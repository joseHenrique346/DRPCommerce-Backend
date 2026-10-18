using System.Security.Claims;
using Commerce.Security;
using DropCommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DropCommerce.Infrastructure.Data.Providers;

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

        if (context?.Items[SecurityNames.ResolvedTenant] is long resolved && resolved > 0)
        {
            var tokenTenant = context.User.FindFirst("EnterpriseId")?.Value;

            if (tokenTenant is not null && tokenTenant != resolved.ToString(System.Globalization.CultureInfo.InvariantCulture))
                throw new SecurityFailure(403, "Security.TenantMismatch", "Token não pertence à loja do domínio.");

            return resolved;
        }

        if (context?.User.Identity?.IsAuthenticated == true && long.TryParse(context.User.FindFirst("EnterpriseId")?.Value, out var id) && id > 0)
            return id;

        throw new SecurityFailure(401, "Security.TenantRequired", "Contexto autenticado de loja necessário.");
    }
}
