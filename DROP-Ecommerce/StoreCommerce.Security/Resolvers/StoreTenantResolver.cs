using SharedCommerce.Application.Interfaces.Tenancy;
using StoreCommerce.Application.Interfaces.Identity;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using SharedCommerce.Application.Options;
using StoreCommerce.Application.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace StoreCommerce.Security.Resolvers;

/// <summary>
/// Resolve a loja e autentica chamadas internas antes de permitir acesso aos dados do Store.
/// </summary>
public sealed class StoreTenantResolver(
    IIdentityRepositories data, IOptions<SecurityOptions> settings,
    StoreCommerce.Application.Interfaces.StoreManagement.IEnterpriseDomainAccess domains) : IRequestTenantResolver
{
    /// <summary>
    /// Valida chamadas internas ou identifica a loja pelo domínio verificado, impedindo o uso de um tenant não confiável.
    /// </summary>
    public async Task ResolveAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/internal"))
        {
            var key = settings.Value.InternalServiceKey;

            if (key.Length < 32 || !OpaqueToken.EqualsSecret(key, context.Request.Headers["X-Service-Key"].ToString()))
                throw new SecurityFailure(401, "Security.InvalidService", "Autenticação de serviço necessária.");

            if (long.TryParse(context.Request.Headers["X-Enterprise-Id"], out var tenant) && tenant > 0)
            {
                if (!await data.Enterprises.ExistsAsync(x => x.Id == tenant && x.IsActive, context.RequestAborted, ignoreQueryFilters: true))
                    throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

                context.Items[SecurityNames.TrustedServiceTenant] = tenant;
            }

            return;
        }

        if (!context.Request.Path.StartsWithSegments("/api/storefront"))
            return;

        var host = HostName.Normalize(context.Request.Host.Host);
        var tenantId = await domains.ResolveAsync(host, null, context.RequestAborted);

        if (tenantId is null)
            throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        context.Items[SecurityNames.ResolvedTenant] = tenantId.Value;
    }
}
