using SharedCommerce.Infrastructure.Data;
using SharedCommerce.Infrastructure.Data.Repositories;
using SharedCommerce.Infrastructure.Data.Context;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.StaticEntity;
using DropCommerce.Application.Appearance;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Interfaces;
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
        services.AddSingleton<StoreIntegrationScope>();
        services.AddSharedInfrastructure();
        services.AddScoped<IAppearanceContext, DropAppearanceContext>();
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
            AddStoreIntegration(services, storeApiBaseUrl);

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
