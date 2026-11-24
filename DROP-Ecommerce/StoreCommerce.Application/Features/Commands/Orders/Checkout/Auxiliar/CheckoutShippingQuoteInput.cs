using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Domain.Entity;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StoreCommerce.Application.Features.Commands;

internal sealed record CheckoutShippingQuoteInput(string OriginPostalCode, string DestinationPostalCode,
    IReadOnlyList<MelhorEnvioQuoteProduct> Products, string Fingerprint)
{
    public static CheckoutShippingQuoteInput Create(IReadOnlyList<CartItem> selected, CheckoutDeliveryAddress destination,
        string originPostalCode, string currencyCode)
    {
        if (currencyCode != StoreSettings.DefaultCurrencyCode)
            throw new SecurityFailure(409, "Checkout.ShippingCurrency", "A cotação do Melhor Envio requer valores em reais.");

        var origin = NormalizePostalCode(originPostalCode, "Checkout.ShippingOriginRequired", "Cadastre um CEP brasileiro válido para a loja.");
        var postalCode = NormalizePostalCode(destination.ZipCode, "Checkout.InvalidShippingPostalCode", "Informe um CEP brasileiro válido para a entrega.");
        var products = selected.Where(Item => !Item.Product.IsDigital)
            .Select(Item => new MelhorEnvioQuoteProduct(Item.ProductVariantId, Item.ProductVariant.Weight ?? Item.Product.Weight,
                Item.ProductVariant.Height ?? Item.Product.Height, Item.ProductVariant.Width ?? Item.Product.Width,
                Item.ProductVariant.Length ?? Item.Product.Length, Item.ProductVariant.Price ?? Item.Product.Price, Item.Quantity))
            .OrderBy(Product => Product.VariantId).ToArray();

        if (products.Any(Product => Product.WeightKg <= 0 || Product.HeightCm <= 0 || Product.WidthCm <= 0 ||
            Product.LengthCm <= 0 || Product.Quantity <= 0))
            throw new SecurityFailure(409, "Checkout.ShippingDimensionsRequired", "A loja precisa cadastrar peso e dimensões dos SKUs selecionados.");

        var fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        {
            origin,
            destination = new { AddressLine = destination.AddressLine.Trim(), City = destination.City.Trim(), destination.StateId, postalCode },
            currencyCode,
            items = selected.OrderBy(Item => Item.Id).Select(Item => new { Item.Id, Item.ProductVariantId, Item.Quantity }),
            products
        })));

        return new CheckoutShippingQuoteInput(origin, postalCode, products, fingerprint);
    }

    private static string NormalizePostalCode(string value, string code, string message)
    {
        var postalCode = value.Trim();

        if (!Regex.IsMatch(postalCode, "^[0-9]{5}-?[0-9]{3}$", RegexOptions.CultureInvariant))
            throw new SecurityFailure(400, code, message);

        return postalCode.Replace("-", "", StringComparison.Ordinal);
    }
}
