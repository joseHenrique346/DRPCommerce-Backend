using System.Security.Cryptography;
using System.Text;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Security;

public static class DropPurchaseIdentity
{
    public static string Create(string secret, long tenant, long eventId, string cpf)
    {
        if (secret.Length < 32)
            throw new SecurityFailure(503, "DropPurchase.IdentityNotConfigured", "Configure a chave privada da franquia de compra.");

        return Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{tenant}:{eventId}:{NormalizeCpf(cpf)}")));
    }

    public static string NormalizeCpf(string cpf)
    {
        var digits = new string((cpf ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

        if (digits.Length != 11 || digits.Distinct().Count() == 1)
            throw new SecurityFailure(400, "DropPurchase.InvalidCpf", "CPF inválido.");

        for (var length = 9; length <= 10; length++)
        {
            var sum = Enumerable.Range(0, length).Sum(Index => (digits[Index] - '0') * (length + 1 - Index));
            var check = 11 - sum % 11;

            if ((check >= 10 ? 0 : check) != digits[length] - '0')
                throw new SecurityFailure(400, "DropPurchase.InvalidCpf", "CPF inválido.");
        }

        return digits;
    }
}
