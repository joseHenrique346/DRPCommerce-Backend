using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace StoreCommerce.Application.Integrations;

public sealed class ProviderAuthorizationSecurity(IOptions<ProviderIntegrationOptions> options)
{
    public static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    public static string Random()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    public void RequireKeyRing()
    {
        if (string.IsNullOrWhiteSpace(options.Value.KeyRingPath))
            throw new SecurityFailure(409, "Integration.KeyRingRequired", "Configure o armazenamento persistente das chaves de integração.");
    }
}
