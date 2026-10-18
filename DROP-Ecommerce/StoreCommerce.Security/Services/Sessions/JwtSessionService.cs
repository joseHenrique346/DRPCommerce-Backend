using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Commerce.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Security.Services;

/// <summary>
/// Emite JWTs e valida o estado das sessões para manter autenticação e revogação consistentes.
/// </summary>
public sealed class JwtSessionService(IIdentityRepositories data, IOptions<SecurityOptions> settings, TimeProvider time) : ISessionValidator, ISessionTokenIssuer
{
    private SecurityOptions Options => settings.Value;
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Formata identificadores nas claims sem depender do idioma do servidor, evitando leituras diferentes do token.
    /// </summary>
    private static Claim Number(string name, long value) => new(name, value.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Emite o JWT administrativo com a identidade, a loja e as permissões do vínculo selecionado.
    /// </summary>
    public async Task<(string Token, DateTime Expires)> IssueUserAsync(User user, Guid familyId, EnterpriseMembership? membership, CancellationToken ct)
    {
        var claims = new List<Claim> { Number("sub", user.Id), new(SecurityNames.Actor, "user"),
            Number(SecurityNames.SecurityVersion, user.SecurityVersion), new(SecurityNames.Session, familyId.ToString()) };

        if (membership is not null)
        {
            claims.AddRange([Number(SecurityNames.Enterprise, membership.EnterpriseId), Number(SecurityNames.Membership, membership.Id),
                Number(SecurityNames.AccessVersion, membership.AccessVersion),
                new(SecurityNames.Kind, membership.KindId switch { 1 => "primary_owner", 2 => "co_owner", _ => "staff" })]);

            if (membership.KindId != 1)
            {
                var permissions = await data.EnterpriseMemberships.GetPermissionsAsync(membership.Id, membership.EnterpriseId, ct);
                claims.AddRange(permissions.Select(code => new Claim(SecurityNames.Permission, code)));
            }

            var employeeId = await data.EnterpriseMemberships.GetEmployeeIdAsync(membership.Id, ct);

            if (employeeId.HasValue)
                claims.Add(Number("EmployeeId", employeeId.Value));
        }

        return Issue(SecurityNames.Backoffice, claims);
    }

    /// <summary>
    /// Emite o JWT do cliente vinculado à sua loja para separar o acesso da vitrine do acesso administrativo.
    /// </summary>
    public (string Token, DateTime Expires) IssueCustomer(Customer customer, Guid familyId) =>
        Issue(SecurityNames.Storefront, [Number("sub", customer.Id), Number(SecurityNames.Customer, customer.Id),
            Number(SecurityNames.Enterprise, customer.EnterpriseId), new(SecurityNames.Actor, "customer"),
            Number(SecurityNames.SecurityVersion, customer.SecurityVersion), new(SecurityNames.Session, familyId.ToString())]);

    /// <summary>
    /// Assina o JWT com prazo e público definidos para impedir adulteração e limitar o uso do acesso.
    /// </summary>
    private (string, DateTime) Issue(string audience, IEnumerable<Claim> claims)
    {
        var now = Now;
        var expires = now.AddMinutes(Options.AccessTokenMinutes);
        var token = new JwtSecurityToken(Options.Issuer, audience,
            claims.Append(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())), now, expires,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SigningKey)), SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    /// <summary>
    /// Valida assinatura, emissor, público e validade do JWT para rejeitar tokens adulterados ou expirados.
    /// </summary>
    public ClaimsPrincipal ValidateSignature(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken) || rawToken.Length > 32768)
            throw new SecurityFailure(401, "Identity.InvalidToken", "Token inválido.");

        try
        {
            return new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(rawToken, new()
            {
                ValidateIssuer = true,
                ValidIssuer = Options.Issuer,
                ValidateAudience = true,
                ValidAudiences = [SecurityNames.Backoffice, SecurityNames.Storefront],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SigningKey)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.FromSeconds(15)
            }, out _);
        }
        catch (SecurityTokenException)
        {
            throw new SecurityFailure(401, "Identity.InvalidToken", "Token inválido.");
        }
        catch (ArgumentException)
        {
            throw new SecurityFailure(401, "Identity.InvalidToken", "Token inválido.");
        }
    }

    /// <summary>
    /// Confere conta, sessão, vínculo e loja no banco para invalidar imediatamente acessos revogados ou alterados.
    /// </summary>
    public async Task<bool> ValidateAsync(ClaimsPrincipal principal, string? host, CancellationToken ct)
    {
        try
        {
            var id = SecurityNames.Id(principal, "sub");
            var version = SecurityNames.Id(principal, SecurityNames.SecurityVersion);

            if (!Guid.TryParse(principal.FindFirst(SecurityNames.Session)?.Value, out var family))
                return false;

            var now = Now;

            if (principal.FindFirst(SecurityNames.Actor)?.Value == "user")
            {
                if (!principal.HasClaim("aud", SecurityNames.Backoffice))
                    return false;

                var user = await data.Users.FindAsync(x => x.Id == id, ct, tracking: false);

                if (user is null || !user.IsActive || user.SecurityVersion != version || (Options.RequireConfirmedEmail && !user.EmailConfirmed))
                    return false;

                if (!await data.UserRefreshTokens.ExistsAsync(x => x.UserId == id && x.FamilyId == family && x.RevokedAt == null && x.FamilyExpiresAt > now, ct)
                    || await data.UserRefreshTokens.ExistsAsync(x => x.UserId == id && x.FamilyId == family && x.RevokedAt != null, ct))
                    return false;

                if (principal.FindFirst(SecurityNames.Enterprise) is null)
                    return principal.FindFirst(SecurityNames.Membership) is null;

                var tenantId = SecurityNames.Id(principal, SecurityNames.Enterprise);
                var membershipId = SecurityNames.Id(principal, SecurityNames.Membership);
                var accessVersion = SecurityNames.Id(principal, SecurityNames.AccessVersion);
                var member = await data.EnterpriseMemberships.FindAsync(x => x.Id == membershipId && x.UserId == id && x.EnterpriseId == tenantId && x.IsActive && x.Enterprise.IsActive, ct, ignoreQueryFilters: true, tracking: false);

                return member is not null && member.AccessVersion == accessVersion &&
                    principal.FindFirst(SecurityNames.Kind)?.Value == (member.KindId switch
                    {
                        1 => "primary_owner",
                        2 => "co_owner",
                        _ => "staff"
                    });
            }

            if (principal.FindFirst(SecurityNames.Actor)?.Value != "customer" || !principal.HasClaim("aud", SecurityNames.Storefront))
                return false;

            var tenant = SecurityNames.Id(principal, SecurityNames.Enterprise);

            if (SecurityNames.Id(principal, SecurityNames.Customer) != id || string.IsNullOrWhiteSpace(host))
                return false;

            var normalizedHost = HostName.Normalize(host);

            if (!await data.EnterpriseDomains.ExistsAsync(x => x.NormalizedHost == normalizedHost && x.EnterpriseId == tenant &&
                x.IsVerified && x.IsActive && x.Enterprise.IsActive, ct, ignoreQueryFilters: true))
                return false;

            var customer = await data.Customers.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true, tracking: false);

            if (customer is null || !customer.IsActive || customer.IsDeleted || customer.SecurityVersion != version || (Options.RequireConfirmedEmail && !customer.EmailConfirmed))
                return false;

            return await data.CustomerRefreshTokens.ExistsAsync(x => x.CustomerId == id && x.EnterpriseId == tenant &&
                x.FamilyId == family && x.RevokedAt == null && x.FamilyExpiresAt > now, ct, ignoreQueryFilters: true) &&
                !await data.CustomerRefreshTokens.ExistsAsync(x => x.CustomerId == id && x.EnterpriseId == tenant && x.FamilyId == family && x.RevokedAt != null, ct, ignoreQueryFilters: true);
        }
        catch (SecurityFailure)
        {
            return false;
        }
    }
}
