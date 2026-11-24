using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using SharedCommerce.Application.Interfaces.Appearance;
using SharedCommerce.Application.Interfaces.Media;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Application.Interfaces.Billing;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Inventory;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Billing;
using SharedCommerce.Infrastructure.Data;
using SharedCommerce.Infrastructure.Data.Repositories;
using SharedCommerce.Infrastructure.Data.Context;
using SharedCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Providers;
using StoreCommerce.Infrastructure.Data.Repositories;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Inventory;
using StoreCommerce.Domain.Entity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Infrastructure.Data.Integrations;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;
using StoreCommerce.Infrastructure.Data.Integrations.MelhorEnvio;

namespace StoreCommerce.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddStoreInfrastructure(this IServiceCollection services, string connectionString, IConfiguration? configuration = null)
    {
        services.AddDbContext<StoreCommerceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddSharedInfrastructure();
        services.AddScoped<StoreCommerce.Application.Interfaces.Appearance.IEnterpriseAppearanceReader, EnterpriseAppearanceReader>();
        services.AddScoped<IAppearanceContext, StoreAppearanceContext>();
        services.AddScoped<IAppearanceEntitlements, StoreAppearanceEntitlements>();
        services.AddScoped<IMediaAssetReader, StoreMediaAssetReader>();
        services.AddScoped<StoreCommerce.Application.Media.MediaWriteScope>();
        services.AddScoped<StoreCommerce.Application.Interfaces.Media.IMediaAssetStore, MediaAssetStore>();
        services.AddScoped<IAppearanceResources, StoreAppearanceResources>();
        services.AddScoped<IAppearanceRepository>(provider => new AppearanceRepository<StoreCommerceDbContext>(
            provider.GetRequiredService<StoreCommerceDbContext>(),
            provider.GetRequiredService<AppearanceWriteScope>(),
            AppearanceChannel.Store,
            (tenant, ct) => provider.GetRequiredService<StoreCommerceDbContext>().Database
                .ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct)));
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        services.AddScoped<IIdentityUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IEntityRepository<>), typeof(EntityRepository<>));
        services.AddScoped<IIdentityRepositories, IdentityRepositories>();
        services.AddScoped<StoreCommerce.Application.Contracts.EnterpriseContractWriteScope>();
        services.AddScoped<StoreCommerce.Application.Contracts.DropContractAcknowledgmentScope>();
        services.AddScoped<StoreCommerce.Application.Contracts.DropStockTransferScope>();
        services.AddScoped<DropSalePaymentWriteScope>();
        services.AddScoped<IDropSaleGateway, MercadoPagoDropSaleGateway>();
        services.AddHostedService<Workers.DropSalePaymentReconciliationWorker>();
        services.AddScoped<StoreCommerce.Application.Contracts.OwnershipTransferWriteScope>();
        services.AddScoped<StoreCommerce.Application.Contracts.ProductReactivationScope>();
        services.AddScoped<StoreCommerce.Application.Interfaces.Commercial.IEnterpriseContractUsageReader, EnterpriseContractUsageReader>();
        services.AddScoped<StoreCommerce.Application.Interfaces.StoreManagement.IStoreActivityReader, StoreActivityReader>();
        services.AddScoped<StoreCommerce.Application.Interfaces.StoreManagement.IEnterpriseAccessReader, EnterpriseAccessReader>();
        services.AddScoped<IAuthenticationSessionRepository, AuthenticationSessionRepository>();
        services.AddScoped<IAccountDeletionStore, AccountDeletionStore>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRepository<Product>>(Provider => Provider.GetRequiredService<IProductRepository>());
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<InventoryWriteScope>();
        services.AddScoped<CartWriteScope>();
        services.AddScoped<StoreCommerce.Application.Orders.OrderWorkflowScope>();
        services.AddScoped<StoreCommerce.Application.Carts.CustomerAddressWriteScope>();
        services.AddScoped<IntegrationWriteScope>();
        services.AddScoped<IProviderConnectionStore, ProviderConnectionStore>();
        services.AddScoped<IProviderTokenProtector, ProviderTokenProtector>();
        services.AddScoped<IPaymentAttemptStore, Payments.PaymentAttemptStore>();
        services.AddScoped<IPaymentOperationStore, Payments.PaymentOperationStore>();
        services.AddHostedService<Workers.PaymentOperationReconciliationWorker>();
        services.AddHostedService<Workers.PaymentAttemptReconciliationWorker>();

        var integrationOptions = configuration?.GetSection("ProviderIntegrations");

        if (integrationOptions is not null)
            services.Configure<ProviderIntegrationOptions>(integrationOptions);
        else
            services.AddOptions<ProviderIntegrationOptions>();

        var protection = services.AddDataProtection().SetApplicationName("DRPCommerce.StoreIntegrations");
        var keyRingPath = integrationOptions?["KeyRingPath"];
        var certificate = integrationOptions?["KeyEncryptionCertificateThumbprint"];

        if (!string.IsNullOrWhiteSpace(keyRingPath))
            protection.PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));

        if (!string.IsNullOrWhiteSpace(certificate))
            protection.ProtectKeysWithCertificate(certificate);
        else if (OperatingSystem.IsWindows())
            protection.ProtectKeysWithDpapi();

        services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
        services.AddOptions<MercadoPagoOptions>();

        if (configuration is not null)
            services.Configure<MercadoPagoOptions>(configuration.GetSection("MercadoPago"));

        services.AddHttpClient("MercadoPago", Client =>
        {
            Client.BaseAddress = new Uri("https://api.mercadopago.com");
            Client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<MercadoPagoApi>();
        services.AddOptions<MelhorEnvioOptions>()
            .Validate(Options => !Options.ShippingEnvironment.HasValue || Enum.IsDefined(Options.ShippingEnvironment.Value),
                "MelhorEnvio:ShippingEnvironment deve ser Sandbox ou Production.");

        if (configuration is not null)
            services.Configure<MelhorEnvioOptions>(configuration.GetSection("MelhorEnvio"));

        services.AddHttpClient<MelhorEnvioAuthorizationClient>(Client =>
        {
            Client.Timeout = TimeSpan.FromSeconds(25);
            Client.MaxResponseContentBufferSize = 256 * 1024;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        });
        services.AddScoped<IMelhorEnvioAuthorizationClient>(Provider => Provider.GetRequiredService<MelhorEnvioAuthorizationClient>());
        services.AddScoped<IProviderTokenRefresher>(Provider => Provider.GetRequiredService<MelhorEnvioAuthorizationClient>());
        services.AddHttpClient<IMelhorEnvioQuoteClient, MelhorEnvioQuoteClient>(Client =>
        {
            Client.Timeout = TimeSpan.FromSeconds(25);
            Client.MaxResponseContentBufferSize = 256 * 1024;
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        });

        services.AddOptions<PlatformBillingOptions>();

        if (configuration is not null)
            services.Configure<PlatformBillingOptions>(configuration.GetSection("PlatformBilling"));

        services.AddScoped<StoreCommerce.Application.Interfaces.Billing.IPlatformBillingProvider, Billing.MercadoPagoPlatformBillingProvider>();
        services.AddScoped<Billing.PlatformBillingSignature>();
        services.AddScoped<IPlatformBillingSignature>(Provider => Provider.GetRequiredService<Billing.PlatformBillingSignature>());
        services.AddScoped<IPlatformBillingNotificationRepository, PlatformBillingNotificationRepository>();
        services.AddScoped<StoreCommerce.Application.Billing.PlatformBillingWriteScope>();
        services.AddScoped<StoreCommerce.Application.Interfaces.Billing.IPlatformSubscriptionProvider, Billing.MercadoPagoSubscriptionProvider>();
        services.AddHostedService<Workers.PlatformBillingWorker>();
        services.AddScoped<MercadoPagoAccess>();
        services.AddScoped<MercadoPagoGateway>();
        services.AddScoped<IPaymentGateway>(Provider => Provider.GetRequiredService<MercadoPagoGateway>());
        services.AddScoped<StoreCommerce.Application.Interfaces.Payments.IPaymentGateway, Payments.ManualPaymentGateway>();
        services.AddScoped<IPaymentGatewayRegistry, PaymentGatewayRegistry>();
        services.AddScoped<IProviderTokenRefresher, MercadoPagoTokenRefresher>();
        services.AddScoped<StoreCommerce.Application.Interfaces.Integrations.IMercadoPagoAuthorizationClient, MercadoPagoAuthorizationClient>();
        services.AddScoped<StoreCommerce.Application.Interfaces.Integrations.IProviderAuthorizationStore, ProviderAuthorizationStore>();
        services.AddScoped<MercadoPagoSignature>();
        services.AddScoped<StoreCommerce.Application.Interfaces.Integrations.IProviderNotificationStore, ProviderNotificationStore>();
        services.AddScoped<IMercadoPagoPaymentReader>(Provider => Provider.GetRequiredService<MercadoPagoGateway>());
        services.AddHostedService<Workers.MercadoPagoReconciliationWorker>();
        services.AddHostedService<Workers.PendingOrderExpirationWorker>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddHostedService<Workers.StockReservationExpirationWorker>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEnterpriseRepository, EnterpriseRepository>();
        services.AddScoped<IEnterpriseMembershipRepository, EnterpriseMembershipRepository>();
        services.AddScoped<IEnterpriseDomainRepository, EnterpriseDomainRepository>();
        services.AddScoped<IAccessRoleRepository, AccessRoleRepository>();
        services.AddScoped<IEmployeePositionRepository, EmployeePositionRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IAccessRolePermissionRepository, AccessRolePermissionRepository>();
        services.AddScoped<IMembershipAccessRoleRepository, MembershipAccessRoleRepository>();

        services.AddScoped<StoreCommerce.Application.Interfaces.Inventory.IDropStockTransferRepository, DropStockTransferRepository>();

        services.AddScoped<StoreCommerce.Application.Interfaces.Payments.IDropSalePaymentRepository, DropSalePaymentRepository>();

        services.AddScoped<StoreCommerce.Application.Interfaces.Billing.IPlatformBillingRepository, PlatformBillingRepository>();

        services.AddScoped<StoreCommerce.Application.Interfaces.Integrations.IDropContractPublicationRepository, DropContractPublicationRepository>();

        return services;
    }
}
