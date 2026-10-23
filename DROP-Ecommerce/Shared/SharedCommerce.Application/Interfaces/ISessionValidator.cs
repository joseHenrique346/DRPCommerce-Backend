using System.Security.Claims;

namespace SharedCommerce.Application.Interfaces;

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
