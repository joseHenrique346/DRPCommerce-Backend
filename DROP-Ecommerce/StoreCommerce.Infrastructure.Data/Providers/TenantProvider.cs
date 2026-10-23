using Microsoft.AspNetCore.Http;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Providers;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long GetEnterpriseId()
    {
        var context = _httpContextAccessor.HttpContext;

        if (context?.Items[SharedCommerce.Application.Security.SecurityNames.TrustedServiceTenant] is long serviceTenant && serviceTenant > 0)
            return serviceTenant;

        if (context?.Items[SharedCommerce.Application.Security.SecurityNames.ResolvedTenant] is long resolvedTenant && resolvedTenant > 0)
        {
            var tokenTenant = context.User.FindFirst("EnterpriseId");

            if (tokenTenant is not null && tokenTenant.Value != resolvedTenant.ToString(System.Globalization.CultureInfo.InvariantCulture))
                throw new SharedCommerce.Domain.Exceptions.SecurityFailure(403, "Security.TenantMismatch", "Token não pertence à loja do domínio.");

            return resolvedTenant;
        }

        if (context?.User.Identity?.IsAuthenticated == true && long.TryParse(context.User.FindFirst("EnterpriseId")?.Value, out var id) && id > 0)
            return id;

        throw new SharedCommerce.Domain.Exceptions.SecurityFailure(401, "Security.TenantRequired", "Contexto autenticado de loja necessário.");
    }
}
