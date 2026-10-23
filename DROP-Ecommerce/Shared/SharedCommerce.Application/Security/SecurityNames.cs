using System.Globalization;
using System.Security.Claims;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Security;

/// <summary>
/// Centraliza nomes de claims e políticas para evitar interpretações diferentes da identidade entre os módulos.
/// </summary>
public static class SecurityNames
{
    public const string Backoffice = "backoffice";
    public const string Storefront = "storefront";
    public const string GlobalUser = "GlobalUser";
    public const string PrimaryOwner = "PrimaryOwnerOnly";
    public const string Actor = "actor_type";
    public const string Enterprise = "EnterpriseId";
    public const string Membership = "MembershipId";
    public const string Customer = "CustomerId";
    public const string Session = "session_id";
    public const string SecurityVersion = "security_version";
    public const string AccessVersion = "access_version";
    public const string Kind = "membership_kind";
    public const string Permission = "permission";
    public const string ResolvedTenant = "Commerce.ResolvedTenant";
    public const string TrustedServiceTenant = "Commerce.ServiceTenant";

    /// <summary>
    /// Lê um identificador válido das claims e rejeita contextos incompletos para evitar acesso sem identidade confiável.
    /// </summary>
    public static long Id(ClaimsPrincipal principal, string claim)
    {
        if (!long.TryParse(principal.FindFirst(claim)?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id < 1)
            throw new SecurityFailure(401, "Security.InvalidContext", "Contexto de autenticação inválido.");

        return id;
    }

    /// <summary>
    /// Confere a permissão administrativa ou o vínculo de dono principal antes de liberar uma operação.
    /// </summary>
    public static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        return user.FindFirst(Actor)?.Value == "user" &&
            (user.FindFirst(Kind)?.Value == "primary_owner" || user.HasClaim(Permission, permission));
    }
}
