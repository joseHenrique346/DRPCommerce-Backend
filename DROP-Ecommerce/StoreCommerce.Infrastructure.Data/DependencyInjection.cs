using StoreCommerce.Application.Billing;
using SharedCommerce.Infrastructure.Data;
using SharedCommerce.Infrastructure.Data.Repositories;
using SharedCommerce.Infrastructure.Data.Context;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Interfaces;
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

namespace StoreCommerce.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddStoreInfrastructure(this IServiceCollection services, string connectionString, IConfiguration? configuration = null)
    {
        services.AddDbContext<StoreCommerceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddSharedInfrastructure();
        services.AddScoped<IAppearanceContext, StoreAppearanceContext>();
        services.AddScoped<IAppearanceEntitlements, StoreAppearanceEntitlements>();
        services.AddScoped<IMediaAssetReader, StoreMediaAssetReader>();
        services.AddScoped<StoreCommerce.Application.Media.MediaWriteScope>();
        services.AddScoped<StoreCommerce.Application.Media.IMediaAssetStore, MediaAssetStore>();
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
        services.AddScoped<StoreCommerce.Application.Contracts.IEnterpriseContractUsageReader, EnterpriseContractUsageReader>();
        services.AddScoped<StoreCommerce.Application.Contracts.IStoreActivityReader, StoreActivityReader>();
        services.AddScoped<StoreCommerce.Application.Contracts.IEnterpriseDomainAccess, EnterpriseDomainAccess>();
        services.AddScoped<StoreCommerce.Application.Contracts.IEnterpriseAccessReader, EnterpriseAccessReader>();
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
        services.AddOptions<PlatformBillingOptions>();

        if (configuration is not null)
            services.Configure<PlatformBillingOptions>(configuration.GetSection("PlatformBilling"));

        services.AddScoped<StoreCommerce.Application.Billing.IPlatformBillingProvider, Billing.MercadoPagoPlatformBillingProvider>();
        services.AddScoped<Billing.PlatformBillingSignature>();
        services.AddScoped<IPlatformBillingSignature>(Provider => Provider.GetRequiredService<Billing.PlatformBillingSignature>());
        services.AddScoped<IPlatformBillingNotificationRepository, PlatformBillingNotificationRepository>();
        services.AddScoped<StoreCommerce.Application.Billing.PlatformBillingWriteScope>();
        services.AddScoped<StoreCommerce.Application.Billing.IPlatformSubscriptionProvider, Billing.MercadoPagoSubscriptionProvider>();
        services.AddHostedService<Workers.PlatformBillingWorker>();
        services.AddScoped<MercadoPagoAccess>();
        services.AddScoped<MercadoPagoGateway>();
        services.AddScoped<IPaymentGateway>(Provider => Provider.GetRequiredService<MercadoPagoGateway>());
        services.AddScoped<StoreCommerce.Application.Payments.IPaymentGateway, Payments.ManualPaymentGateway>();
        services.AddScoped<IPaymentGatewayRegistry, PaymentGatewayRegistry>();
        services.AddScoped<IProviderTokenRefresher, MercadoPagoTokenRefresher>();
        services.AddScoped<MercadoPagoAuthorizationService>();
        services.AddScoped<MercadoPagoSignature>();
        services.AddScoped<MercadoPagoNotifications>();
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

        services.AddScoped<StoreCommerce.Application.Contracts.IDropStockTransferRepository, DropStockTransferRepository>();

        services.AddScoped<StoreCommerce.Application.Payments.IDropSalePaymentRepository, DropSalePaymentRepository>();

        services.AddScoped<StoreCommerce.Application.Billing.IPlatformBillingRepository, PlatformBillingRepository>();

        services.AddScoped<StoreCommerce.Application.Contracts.IDropContractPublicationRepository, DropContractPublicationRepository>();

        return services;
    }
}
