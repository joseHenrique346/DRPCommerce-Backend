using Microsoft.AspNetCore.DataProtection;

namespace StoreCommerce.Application.Integrations.MercadoPago;

public sealed class MercadoPagoAuthorizationCryptography(IDataProtectionProvider protection)
{
    public string ProtectVerifier(string verifier)
    {
        return protection.CreateProtector("MercadoPago.OAuth.Pkce.v1").Protect(verifier);
    }

    public string UnprotectVerifier(string verifier)
    {
        return protection.CreateProtector("MercadoPago.OAuth.Pkce.v1").Unprotect(verifier);
    }
}
