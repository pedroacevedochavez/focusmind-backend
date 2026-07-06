using FocusMind.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FocusMind.Business.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, JwtOptions jwtOptions)
    {
        services.AddSingleton(jwtOptions);
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMotorRecomendacionService, MotorRecomendacionService>();
        services.AddScoped<IDiagnosticoService, DiagnosticoService>();

        return services;
    }
}
