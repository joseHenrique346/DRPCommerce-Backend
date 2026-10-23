using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Security;

/// <summary>
/// Cria e compara segredos de uso controlado sem precisar guardar seus valores originais no banco.
/// </summary>
public static class OpaqueToken
{
    /// <summary>
    /// Gera um token aleatório imprevisível para proteger sessões, convites e outras ações de uso controlado.
    /// </summary>
    public static string Create()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
    }

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
    public static bool EqualsSecret(string expected, string provided)
    {
        return !string.IsNullOrWhiteSpace(expected) && !string.IsNullOrWhiteSpace(provided) &&
            CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(expected)), SHA256.HashData(Encoding.UTF8.GetBytes(provided)));
    }
}
