using SharedCommerce.Application;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Features;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStoreApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddSharedApplication(assembly);
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<IdentityHandlerContext>();
        services.AddScoped<ProviderAuthorizationAccessPolicy>();
        services.AddScoped<StoreCommerce.Application.Integrations.MercadoPago.MercadoPagoAuthorizationCryptography>();
        services.AddScoped<StoreSettingsReader>();
        services.AddScoped<CouponCustomerHistoryReader>();
        services.AddScoped<CartPurchaseContextReader>();
        services.AddScoped<CartResponseBuilder>();
        services.AddScoped<Integrations.ProviderConnectionService>();
        services.AddScoped<Integrations.Providers.Interfaces.IProviderConnectionCredentials>(
            Provider => Provider.GetRequiredService<Integrations.ProviderConnectionService>());
        services.AddScoped<CategoryCatalogReader>();
        services.AddScoped<CatalogWriteContext>();
        services.AddScoped<ProductDefaultVariantWriter>();
        services.AddScoped<ProductSearchReader>();
        services.AddScoped<ProductContractStateReader>();
        services.AddScoped<ProductReactivationSelector>();
        services.AddScoped<StockReservationExpirationWriter>();
        services.AddScoped<StockReservationCompletionWriter>();
        services.AddScoped<MediaOwnerWriteAccess>();
        services.AddScoped<OrderStockWriter>();
        services.AddScoped<OrderCouponRedemptionWriter>();
        services.AddScoped<OrderPaymentOperationWriter>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
