using StoreCommerce.Application.Identity;
using Commerce.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace StoreCommerce.Security.Resolvers;

/// <summary>
/// Resolve a loja e autentica chamadas internas antes de permitir acesso aos dados do Store.
/// </summary>
public sealed class StoreTenantResolver(IIdentityRepositories data, IOptions<SecurityOptions> settings) : IRequestTenantResolver
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
        var domain = await data.EnterpriseDomains.FindValueAsync(x => x.NormalizedHost == host && x.IsVerified && x.IsActive && x.Enterprise.IsActive, x => new { x.EnterpriseId }, context.RequestAborted, ignoreQueryFilters: true);

        if (domain is null)
            throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        context.Items[SecurityNames.ResolvedTenant] = domain.EnterpriseId;
    }
}
