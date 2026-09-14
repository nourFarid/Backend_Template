using BLL.JWT;
using BLL.Services;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBllDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. JWT Configuration
        services.Configure<JwtAuthOption>(configuration.GetSection("Jwt"));

        // 2. Mapster Auto-Scanning (Scans all IRegister configs in BLL once)
        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(DependencyInjection).Assembly);
        services.AddSingleton(mapsterConfig);

        // 3. Automatic Service Scanning (Auto-registers all *Service classes as Self & Interfaces)
        services.Scan(scan => scan
            .FromAssemblyOf<AuthService>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
