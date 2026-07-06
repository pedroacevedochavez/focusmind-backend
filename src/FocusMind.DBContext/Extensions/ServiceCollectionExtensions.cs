using FocusMind.DBContext.Common;
using FocusMind.DBContext.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FocusMind.DBContext.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextServices(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IHealthRepository, HealthRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IObjetivoRepository, ObjetivoRepository>();
        services.AddScoped<IAlergenoRepository, AlergenoRepository>();
        services.AddScoped<IDiagnosticoRepository, DiagnosticoRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();

        return services;
    }
}
