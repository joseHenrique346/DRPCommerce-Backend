using DropCommerce.Application.Interfaces.Appearance;
using DropCommerce.Domain.Interfaces.Catalog;
using DropCommerce.Domain.Interfaces.Events;
using DropCommerce.Domain.Interfaces.Orders;
using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Domain.Interfaces.Queue;
using DropCommerce.Infrastructure.Data.Integrations.Store.Interfaces;
using SharedCommerce.Application.Interfaces.Appearance;
using SharedCommerce.Application.Interfaces.Media;
using SharedCommerce.Domain.Interfaces.Tenancy;
using SharedCommerce.Infrastructure.Data;
using SharedCommerce.Infrastructure.Data.Repositories;
using SharedCommerce.Infrastructure.Data.Context;
using SharedCommerce.Domain.StaticEntity;
using DropCommerce.Application.Appearance;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using DropCommerce.Infrastructure.Data.Integrations.Store.Readers;
using DropCommerce.Infrastructure.Data.Providers;
using DropCommerce.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace DropCommerce.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDropInfrastructure(this IServiceCollection services, string connectionString) =>
        AddDropInfrastructure(services, connectionString, null);

    public static IServiceCollection AddDropInfrastructure(this IServiceCollection services, string connectionString, string? storeApiBaseUrl)
    {
        services.AddDbContext<DropCommerceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<DropCommerce.Application.Interfaces.Identity.ICurrentDropActor, CurrentDropActor>();
        services.AddSingleton<StoreIntegrationScope>();
        services.AddSharedInfrastructure();
        services.AddScoped<IAppearanceContext, DropAppearanceContext>();
        services.AddScoped<IAppearanceEntitlements, DropAppearanceEntitlements>();
        services.AddScoped<IStoreAppearanceReader, StoreAppearanceReader>();
        services.AddScoped<IStoreCommercialReader, StoreCommercialReader>();
        services.AddScoped<DropCommerce.Application.Contracts.DropCommercialWriteScope>();
        services.AddOptions<DropCommerce.Application.Contracts.DropQueueOptions>();
        services.AddScoped<IStoreDropPayments, StoreDropPayments>();
        services.AddScoped<IMediaAssetReader, DropMediaAssetReader>();
        services.AddScoped<IAppearanceResources, DropAppearanceResources>();
        services.AddScoped<IDropAppearanceEventRepository, DropAppearanceEventRepository>();
        services.AddScoped<IAppearanceRepository>(provider => new AppearanceRepository<DropCommerceDbContext>(
            provider.GetRequiredService<DropCommerceDbContext>(),
            provider.GetRequiredService<AppearanceWriteScope>(),
            AppearanceChannel.Drop));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IDropEventRepository, DropEventRepository>();
        services.AddScoped<IDropOrderRepository, DropOrderRepository>();
        services.AddScoped<IDropProductRepository, DropProductRepository>();
        services.AddScoped<IDropCouponRepository, DropCouponRepository>();
        services.AddScoped<IDropRegistrationRepository, DropRegistrationRepository>();
        services.AddScoped<IQueueEntryRepository, QueueEntryRepository>();

        if (!string.IsNullOrWhiteSpace(storeApiBaseUrl))
        {
            AddStoreIntegration(services, storeApiBaseUrl);
            services.AddHttpClient("DropContractSync", Client =>
            {
                Client.BaseAddress = new Uri(storeApiBaseUrl);
                Client.Timeout = TimeSpan.FromSeconds(10);
            });
            services.AddHostedService<Workers.DropContractReconciliationWorker>();
            services.AddHostedService<Workers.DropQueueExpirationWorker>();
        }

        services.AddScoped<DropCommerce.Application.Interfaces.Catalog.IDropCatalogRepository, DropCatalogRepository>();

        services.AddScoped<DropCommerce.Application.Interfaces.Queue.IDropQueueRepository, DropQueueRepository>();

        services.AddScoped<DropCommerce.Application.Interfaces.Orders.IDropCheckoutRepository, DropCheckoutRepository>();

        services.AddScoped<DropCommerce.Application.Interfaces.Inventory.IDropStockSagaRepository, DropStockSagaRepository>();

        services.AddScoped<DropCommerce.Application.Interfaces.Storefront.IDropStorefrontRepository, DropStorefrontRepository>();

        return services;
    }

    private static void AddStoreIntegration(IServiceCollection services, string storeApiBaseUrl)
    {
        if (!Uri.TryCreate(storeApiBaseUrl, UriKind.Absolute, out var storeApiUri)
            || (storeApiUri.Scheme != Uri.UriSchemeHttp && storeApiUri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException("A configuração StoreApi:BaseUrl deve conter uma URL absoluta válida.");

        services.AddRefitGeneratedClient<IStoreProductRefitApi>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer() })
            .AddHttpMessageHandler<StoreAuthenticationHandler>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = storeApiUri;
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.DisableForUnsafeHttpMethods();
                options.CircuitBreaker.MinimumThroughput = 10;
            });

        services.AddRefitGeneratedClient<IStoreEnterpriseRefitApi>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer() })
            .AddHttpMessageHandler<StoreAuthenticationHandler>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = storeApiUri;
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.DisableForUnsafeHttpMethods();
                options.CircuitBreaker.MinimumThroughput = 10;
            });

        services.AddRefitGeneratedClient<IStoreCustomerRefitApi>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer() })
            .AddHttpMessageHandler<StoreAuthenticationHandler>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = storeApiUri;
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.DisableForUnsafeHttpMethods();
                options.CircuitBreaker.MinimumThroughput = 10;
            });

        services.AddRefitGeneratedClient<IStoreEmployeeRefitApi>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer() })
            .AddHttpMessageHandler<StoreAuthenticationHandler>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = storeApiUri;
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.DisableForUnsafeHttpMethods();
                options.CircuitBreaker.MinimumThroughput = 10;
            });

        services.AddScoped<IStoreProductReader, StoreProductReader>();
        services.AddScoped<IStoreEnterpriseReader, StoreEnterpriseReader>();
        services.AddScoped<IStoreCustomerReader, StoreCustomerReader>();
        services.AddScoped<IStoreEmployeeReader, StoreEmployeeReader>();
    }
}
