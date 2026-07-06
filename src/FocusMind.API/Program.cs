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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
