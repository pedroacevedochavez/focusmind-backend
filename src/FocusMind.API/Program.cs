using System.Text;
using FocusMind.Business.Extensions;
using FocusMind.Business.Services;
using FocusMind.DBContext.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("FocusMindDb")
    ?? throw new InvalidOperationException("La cadena de conexión 'FocusMindDb' no está configurada en appsettings.json.");
builder.Services.AddDbContextServices(connectionString);

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = new JwtOptions(
    Key: jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key no está configurado en appsettings.json."),
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
        };
    });

builder.Services.AddAuthorization();

// HU-19: habilita al Frontend Angular (servido en otro origen/puerto por `ng serve`) a
// consumir la API. Sin credentials (cookies) porque la sesión viaja como Bearer token en el
// header Authorization, no como cookie — así se puede usar AllowAnyHeader/AllowAnyMethod sin
// entrar en conflicto con la restricción de CORS que impide combinar wildcard + credentials.
// El origen se lee de appsettings.json (Cors:FrontendOrigin) para no hardcodear el puerto;
// por defecto apunta al puerto estándar de `ng serve` (4200) en local.
const string FrontendCorsPolicy = "FrontendLocal";
var frontendOrigin = builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Orden requerido por ASP.NET Core: CORS antes de Authentication/Authorization.
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
