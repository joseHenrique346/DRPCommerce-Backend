using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Integrations.MercadoPago;

/// <summary>
/// Validação criptográfica compartilhada; cada fluxo fornece seu próprio segredo.
/// Não consulta credenciais, conexões ou dados de negócio.
/// </summary>
public static class MercadoPagoWebhookSignature
{
    public static string Verify(string paymentId, string requestId, string signature,
        string secret, int toleranceSeconds, DateTimeOffset now)
    {
        if (paymentId.Length is < 1 or > 200 || paymentId.Any(Character => !char.IsAsciiLetterOrDigit(Character) && Character is not ('-' or '_')) ||
            requestId.Length is < 1 or > 200 || requestId.Any(Character => !char.IsAsciiLetterOrDigit(Character) && Character is not ('-' or '_')) ||
            signature.Length is < 1 or > 512 || string.IsNullOrWhiteSpace(secret))
            throw Invalid();

        var fields = signature.Split(',').Select(Field => Field.Trim().Split('=', 2)).ToList();

        if (fields.Any(Field => Field.Length != 2) || fields.Count(Field => Field[0] == "ts") != 1 || fields.Count(Field => Field[0] == "v1") != 1)
            throw Invalid();

        var timestamp = fields.Single(Field => Field[0] == "ts")[1];
        var digest = fields.Single(Field => Field[0] == "v1")[1];

        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var numeric) ||
            digest.Length != 64 || digest.Any(Character => !Uri.IsHexDigit(Character)))
            throw Invalid();

        DateTimeOffset issued;

        try
        {
            issued = numeric > 1_000_000_000_000 ? DateTimeOffset.FromUnixTimeMilliseconds(numeric) : DateTimeOffset.FromUnixTimeSeconds(numeric);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw Invalid();
        }

        if (Math.Abs((now - issued).TotalSeconds) > Math.Clamp(toleranceSeconds, 60, 3600))
            throw Invalid();

        var manifest = $"id:{paymentId.ToLowerInvariant()};request-id:{requestId};ts:{timestamp};";
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(manifest));

        if (!CryptographicOperations.FixedTimeEquals(expected, Convert.FromHexString(digest)))
            throw Invalid();

        return ProviderAuthorizationSecurity.Hash(requestId + "\n" + paymentId + "\n" + timestamp);
    }

    private static SecurityFailure Invalid()
    {
        return new SecurityFailure(401, "MercadoPago.InvalidSignature", "Assinatura do webhook inválida ou expirada.");
    }
}
