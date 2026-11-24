using Microsoft.AspNetCore.DataProtection;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CheckoutShippingQuoteProtector(IDataProtectionProvider protection, ProviderAuthorizationSecurity security)
{
    public const int MaxTokenLength = 32_768;

    internal string Protect(long enterpriseId, long customerId, CheckoutShippingQuoteToken quote)
    {
        return ForCustomer(enterpriseId, customerId).Protect(JsonSerializer.Serialize(quote));
    }

    internal CheckoutShippingQuoteToken Read(long enterpriseId, long customerId, string token)
    {
        var protector = ForCustomer(enterpriseId, customerId);

        try
        {
            return JsonSerializer.Deserialize<CheckoutShippingQuoteToken>(protector.Unprotect(token)) ?? throw InvalidQuote();
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException or FormatException)
        {
            throw InvalidQuote();
        }
    }

    private IDataProtector ForCustomer(long enterpriseId, long customerId)
    {
        security.RequireKeyRing();

        return protection.CreateProtector("Store.Checkout.ShippingQuote.v1", enterpriseId.ToString(CultureInfo.InvariantCulture),
            customerId.ToString(CultureInfo.InvariantCulture));
    }

    private static SecurityFailure InvalidQuote()
    {
        return new SecurityFailure(409, "Checkout.InvalidShippingQuote", "Cotação inválida. Consulte o frete novamente.");
    }
}
