namespace SharedCommerce.Domain.Exceptions;

/// <summary>
/// Representa uma falha esperada de segurança com código e status para uma resposta HTTP controlada.
/// </summary>
public sealed class SecurityFailure(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}
