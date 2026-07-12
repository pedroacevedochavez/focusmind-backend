using System.Text;
using System.Threading.RateLimiting;
using FocusMind.API.Middleware;
using FocusMind.Business.Extensions;
using FocusMind.Business.Services;
using FocusMind.DBContext.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// HU-20: ningún endpoint de este dominio necesita bodies grandes (el producto más "pesado"
// es un JSON de texto con listas cortas de ingredientes/alérgenos; las imágenes ya se suben
// directo a S3 desde el cliente, HU-22, la API solo recibe la URL resultante). Un límite bajo
// reduce la superficie de un DoS por payload gigante antes de que el body llegue a bind/validar.
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1_000_000; // 1 MB
});

var connectionString = builder.Configuration.GetConnectionString("FocusMindDb")
    ?? throw new InvalidOperationException("La cadena de conexión 'FocusMindDb' no está configurada en appsettings.json.");
builder.Services.AddDbContextServices(connectionString);

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key no está configurado en appsettings.json.");

// HU-20: HMACSHA256 exige una clave de al menos 256 bits (32 bytes/caracteres ASCII) para que
// la firma sea resistente a fuerza bruta; una clave corta debilita todo el esquema de auth sin
// que nada lo detecte en runtime. Falla rápido al arrancar en vez de emitir tokens firmados con
// una clave débil.
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key debe tener al menos 32 bytes (256 bits) para HMACSHA256. Configúrala vía User Secrets " +
        "en local (dotnet user-secrets set \"Jwt:Key\" \"...\") o AWS Secrets Manager en producción — nunca en appsettings.json.");
}

var jwtOptions = new JwtOptions(
    Key: jwtKey,
    Issuer: jwtSection["Issuer"] ?? "FocusMind.API",
    Audience: jwtSection["Audience"] ?? "FocusMind.Frontend",
    AccessTokenMinutes: int.TryParse(jwtSection["AccessTokenMinutes"], out var accessMinutes) ? accessMinutes : 15,
    RefreshTokenDays: int.TryParse(jwtSection["RefreshTokenDays"], out var refreshDays) ? refreshDays : 7);

builder.Services.AddBusinessServices(jwtOptions);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sin esto, JwtSecurityTokenHandler re-mapea claims cortos ("sub", "email") a URIs
        // legacy de .NET (ClaimTypes.NameIdentifier, etc.) al validar el token — entonces
        // User.FindFirst(JwtRegisteredClaimNames.Sub) nunca encontraría el claim que
        // JwtTokenGenerator sí firmó como "sub" literal. Ver también JwtTokenGenerator.Validar.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            // HU-20: fija el algoritmo esperado en vez de confiar en el que declare el propio
            // token — mitiga ataques de "confusión de algoritmo" (p.ej. un token manipulado que
            // declara "alg":"none" o intenta forzar una verificación distinta a la firma real).
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        };
    });

builder.Services.AddAuthorization();

// HU-20: Microsoft.AspNetCore.RateLimiting ya viene en el shared framework (no requiere NuGet
// adicional). Dos políticas, alineadas al criterio de aceptación de HU-20:
//   - "general": 100 solicitudes/IP/15 min, aplicada globalmente vía GlobalLimiter.
//   - "auth": 10 solicitudes/IP/hora, aplicada solo a /api/auth/* (fuerza bruta de login).
// Cuando un endpoint tiene [EnableRateLimiting("auth")], AMBAS políticas se evalúan (la global
// SIEMPRE corre, la de endpoint se suma) — es intencional: los endpoints de auth quedan bajo
// una capa extra de restricción, no bajo una que reemplaza a la otra.
static string ObtenerClaveParticionPorIp(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "desconocido";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(ObtenerClaveParticionPorIp(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(15),
            QueueLimit = 0,
        }));

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(ObtenerClaveParticionPorIp(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
        }));

    options.OnRejected = async (rejectedContext, cancellationToken) =>
    {
        // El propio limitador conoce cuánto falta para la siguiente ventana (15 min en
        // "general", 1 hora en "auth") — se reutiliza en vez de hardcodear un valor que
        // quedaría desalineado con la política que realmente rechazó la solicitud.
        var retryAfterSegundos = rejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? (int)retryAfter.TotalSeconds
            : 60;
        rejectedContext.HttpContext.Response.Headers.RetryAfter = retryAfterSegundos.ToString();
        await rejectedContext.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Demasiadas solicitudes. Intenta de nuevo más tarde." },
            cancellationToken);
    };
});

// HU-19: habilita al Frontend Angular (servido en otro origen/puerto por `ng serve`) a
// consumir la API. Sin credentials (cookies) porque la sesión viaja como Bearer token en el
// header Authorization, no como cookie — así se puede usar AllowAnyHeader/AllowAnyMethod sin
// entrar en conflicto con la restricción de CORS que impide combinar wildcard + credentials.
// El origen se lee de appsettings.json (Cors:FrontendOrigin) para no hardcodear el puerto;
// por defecto apunta al puerto estándar de `ng serve` (4200) en local.
const string FrontendCorsPolicy = "FrontendLocal";
var frontendOrigin = builder.Configuration["Cors:FrontendOrigin"] ?? "https://main.di5bbu5p2ozkb.amplifyapp.com";

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Preparación para Elastic Beanstalk: el Application Load Balancer termina TLS y reenvía la
// petición a la instancia como HTTP simple + cabeceras X-Forwarded-Proto/X-Forwarded-For. Sin
// esto, Kestrel ve toda petición como HTTP y UseHttpsRedirection() de abajo entra en bucle de
// redirección contra un ALB que ya fuerza HTTPS en su listener. KnownIPNetworks/KnownProxies se
// dejan vacíos (en vez de listar la IP del ALB, que no es estática) porque el Security Group de
// la instancia de EB ya restringe quién puede alcanzarla en primer lugar — ver guía de
// despliegue para la regla exacta del Security Group.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Debe ir ANTES de UseHttpsRedirection: reescribe HttpContext.Request.Scheme/RemoteIpAddress a
// partir de X-Forwarded-Proto/X-Forwarded-For para que todo lo que viene después (redirección
// HTTPS, el propio HSTS de SecurityHeadersMiddleware, el partitioning por IP del rate limiter)
// vea los valores reales del cliente y no los del ALB.
app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseMiddleware<SecurityHeadersMiddleware>();

// Orden requerido por ASP.NET Core: CORS antes de Authentication/Authorization. El rate
// limiter se coloca apenas después de CORS y antes de Authentication a propósito: rechazar
// una IP que ya excedió su cuota debe costar lo menos posible (nada de validar JWT primero).
app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
