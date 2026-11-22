using DropCommerce.Application.Contracts;
using DropCommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Providers;

public sealed class CurrentDropActor(ITenantProvider tenants, IHttpContextAccessor http) : ICurrentDropActor
{
    public long EnterpriseId => tenants.GetEnterpriseId();
    public long CustomerId => http.HttpContext?.User.FindFirst(SecurityNames.Actor)?.Value == "customer"
        ? SecurityNames.Id(http.HttpContext.User, SecurityNames.Customer)
        : throw new SecurityFailure(403, "DropPurchase.CustomerRequired", "Autenticação de comprador necessária.");
    public bool IsAuthenticated => http.HttpContext?.User.Identity?.IsAuthenticated == true;
    public bool IsUser => http.HttpContext?.User.FindFirst(SecurityNames.Actor)?.Value == "user";
    public bool HasMembership => http.HttpContext?.User.FindFirst(SecurityNames.Membership) is not null;
    public string? UserAgent => http.HttpContext?.Request.Headers.UserAgent.ToString();
    public string? IpAddress => http.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool HasPermission(string permission)
    {
        return http.HttpContext?.User is { } principal && SecurityNames.HasPermission(principal, permission);
    }
}
