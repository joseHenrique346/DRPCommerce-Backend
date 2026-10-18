using Microsoft.Extensions.Options;
using Commerce.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Security.Context;
using StoreCommerce.Security.Hashers;
using StoreCommerce.Security.Resolvers;
using StoreCommerce.Security.Scopes;
using StoreCommerce.Security.Services;
using StoreCommerce.Security.Delivery;
using StoreCommerce.Security.Verifiers;
using StoreCommerce.Security.Workers;

namespace StoreCommerce.Security;

/// <summary>
/// Registra as implementações de segurança usadas pelos casos de uso de identidade do Store.
/// </summary>
public static class StoreSecurityRegistration
{
    /// <summary>
    /// Registra hash de senha, JWT, contexto e serviços auxiliares para atender os fluxos de identidade do Store.
    /// </summary>
    public static IServiceCollection AddStoreSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommerceSecurity(configuration);
        services.AddScoped<IIdentityOperationScope, IdentityOperationScope>();
        services.AddScoped<IdentityContext>();
        services.AddScoped<ICurrentIdentity>(provider => provider.GetRequiredService<IdentityContext>());
        services.AddScoped(provider => provider.GetRequiredService<IOptions<SecurityOptions>>().Value);
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IPasswordHasher<Customer>, PasswordHasher<Customer>>();
        services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210000);
        services.AddSingleton<IAccountPasswordHasher, AccountPasswordHasher>();
        services.AddScoped<JwtSessionService>();
        services.AddScoped<ISessionTokenIssuer>(provider => provider.GetRequiredService<JwtSessionService>());
        services.AddScoped<ISessionValidator>(sp => sp.GetRequiredService<JwtSessionService>());
        services.AddScoped<IRequestTenantResolver, StoreTenantResolver>();
        services.AddSingleton<IAccountActionDelivery, DisabledAccountActionDelivery>();
        services.AddHttpClient<IDomainOwnershipVerifier, DnsDomainOwnershipVerifier>(client => client.Timeout = TimeSpan.FromSeconds(5));
        services.AddHostedService<TokenCleanupWorker>();
        return services;
    }
}
