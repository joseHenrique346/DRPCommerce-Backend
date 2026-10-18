using StoreCommerce.Application.Identity;
using Commerce.Security;
using Microsoft.AspNetCore.Http;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Security.Context;

/// <summary>
/// Disponibiliza a identidade autenticada e a loja atual sem receber esses privilégios do corpo da requisição.
/// </summary>
public sealed class IdentityContext(IHttpContextAccessor accessor, ITenantProvider tenant) : ICurrentIdentity
{
    public System.Security.Claims.ClaimsPrincipal Principal =>
        accessor.HttpContext?.User ?? throw new SecurityFailure(401, "Identity.Unauthenticated", "Autenticação necessária.");
    public long UserId => Principal.FindFirst(SecurityNames.Actor)?.Value == "user" ? SecurityNames.Id(Principal, "sub")
        : throw new SecurityFailure(403, "Identity.BackofficeRequired", "Acesso administrativo necessário.");
    public long CustomerId => Principal.FindFirst(SecurityNames.Actor)?.Value == "customer" ? SecurityNames.Id(Principal, SecurityNames.Customer)
        : throw new SecurityFailure(403, "Identity.CustomerRequired", "Autenticação de cliente necessária.");
    public long EnterpriseId => tenant.GetEnterpriseId();
    public long MembershipId => SecurityNames.Id(Principal, SecurityNames.Membership);
    public Guid FamilyId => Guid.TryParse(Principal.FindFirst(SecurityNames.Session)?.Value, out var value) ? value
        : throw new SecurityFailure(401, "Identity.InvalidSession", "Sessão inválida.");
    public string Host => accessor.HttpContext?.Request.Host.Host ?? "";
    /// <summary>
    /// Exige o vínculo de dono principal para impedir que sócios ou funcionários alterem operações reservadas ao proprietário.
    /// </summary>
    public void RequirePrimaryOwner()
    {
        if (Principal.FindFirst(SecurityNames.Kind)?.Value != "primary_owner" || Principal.FindFirst(SecurityNames.Actor)?.Value != "user")
            throw new SecurityFailure(403, "Identity.PrimaryOwnerRequired", "Apenas o dono principal pode executar esta operação.");
    }
}
