using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Commerce.Security;

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
    public static bool HasPermission(ClaimsPrincipal user, string permission) =>
        user.FindFirst(Actor)?.Value == "user" &&
        (user.FindFirst(Kind)?.Value == "primary_owner" || user.HasClaim(Permission, permission));
}

/// <summary>
/// Cria e compara segredos de uso controlado sem precisar guardar seus valores originais no banco.
/// </summary>
public static class OpaqueToken
{
    /// <summary>
    /// Gera um token aleatório imprevisível para proteger sessões, convites e outras ações de uso controlado.
    /// </summary>
    public static string Create() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));

    /// <summary>
    /// Calcula o hash do token para reconhecê-lo no banco sem armazenar o segredo original.
    /// </summary>
    public static string Hash(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 512)
            throw new SecurityFailure(400, "Security.InvalidToken", "Token inválido.");

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    /// <summary>
    /// Compara segredos em tempo constante para reduzir vazamentos por diferenças no tempo de resposta.
    /// </summary>
    public static bool EqualsSecret(string expected, string provided) =>
        !string.IsNullOrWhiteSpace(expected) && !string.IsNullOrWhiteSpace(provided) &&
        CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(expected)), SHA256.HashData(Encoding.UTF8.GetBytes(provided)));
}

/// <summary>
/// Representa uma falha esperada de segurança com código e status para uma resposta HTTP controlada.
/// </summary>
public sealed class SecurityFailure(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}

/// <summary>
/// Define a validação da sessão atual para que um JWT válido não garanta acesso depois de uma revogação.
/// </summary>
public interface ISessionValidator
{
    /// <summary>
    /// Define a verificação do estado da sessão para impedir o uso de tokens já revogados.
    /// </summary>
    Task<bool> ValidateAsync(ClaimsPrincipal principal, string? host, CancellationToken cancellationToken);
}

/// <summary>
/// Define a identificação confiável da loja antes do uso dos seus dados.
/// </summary>
public interface IRequestTenantResolver
{
    /// <summary>
    /// Define como identificar a loja da requisição para manter os dados separados entre tenants.
    /// </summary>
    Task ResolveAsync(Microsoft.AspNetCore.Http.HttpContext context);
}

/// <summary>
/// Padroniza domínios para identificar lojas sem ambiguidades ou formatos indevidos.
/// </summary>
public static class HostName
{
    /// <summary>
    /// Padroniza e valida o domínio para comparar lojas sem aceitar protocolo, porta ou caminho indevidos.
    /// </summary>
    public static string Normalize(string host)
    {
        var value = host.Trim().TrimEnd('.');

        if (value.Contains('/') || value.Contains(':') || value.Contains('@') || value.Length > 253)
            throw new SecurityFailure(400, "Domain.InvalidHost", "Informe um domínio sem protocolo, porta ou caminho.");

        try
        {
            value = new IdnMapping().GetAscii(value).ToLowerInvariant();
        }
        catch (ArgumentException)
        {
            throw new SecurityFailure(400, "Domain.InvalidHost", "Domínio inválido.");
        }

        if (Uri.CheckHostName(value) != UriHostNameType.Dns || value.Split('.').Any(label => label.Length is 0 or > 63 || label.StartsWith('-') || label.EndsWith('-')))
            throw new SecurityFailure(400, "Domain.InvalidHost", "Domínio inválido.");

        return value;
    }
}
