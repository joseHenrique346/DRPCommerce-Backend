using System.Security.Claims;
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
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("EnterpriseId");
        return claim is not null && long.TryParse(claim.Value, out var id) ? id : 0;
    }
}
