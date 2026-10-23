using Microsoft.Extensions.DependencyInjection;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Infrastructure.Data.Context;
using SharedCommerce.Infrastructure.Data.Providers;
using SharedCommerce.Infrastructure.Data.Workers;

namespace SharedCommerce.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<AppearanceWriteScope>();
        services.AddScoped<IMediaAssetReader, DisabledMediaAssetReader>();
        services.AddSingleton(TimeProvider.System);
        services.AddHostedService<AppearanceMaintenanceWorker>();

        return services;
    }
}
