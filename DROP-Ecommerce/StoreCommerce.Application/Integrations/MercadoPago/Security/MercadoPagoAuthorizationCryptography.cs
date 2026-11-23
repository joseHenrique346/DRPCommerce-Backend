using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace StoreCommerce.Application.Integrations.MercadoPago;

public sealed class MercadoPagoAuthorizationCryptography(
    IDataProtectionProvider protection,
    IOptions<ProviderIntegrationOptions> integrationOptions)
{
    public static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    public static string Random()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    public string ProtectVerifier(string verifier)
    {
        return protection.CreateProtector("MercadoPago.OAuth.Pkce.v1").Protect(verifier);
    }

    public string UnprotectVerifier(string verifier)
    {
        return protection.CreateProtector("MercadoPago.OAuth.Pkce.v1").Unprotect(verifier);
    }

    public void RequireKeyRing()
    {
        if (string.IsNullOrWhiteSpace(integrationOptions.Value.KeyRingPath))
            throw new SecurityFailure(409, "Integration.KeyRingRequired", "Configure o armazenamento persistente das chaves de integração.");
    }
}
