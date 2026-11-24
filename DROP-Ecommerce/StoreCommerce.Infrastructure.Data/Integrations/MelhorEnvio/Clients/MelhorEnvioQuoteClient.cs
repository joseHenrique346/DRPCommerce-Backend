using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Domain.Entity;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StoreCommerce.Infrastructure.Data.Integrations.MelhorEnvio;

public sealed class MelhorEnvioQuoteClient(HttpClient http, IOptions<MelhorEnvioOptions> options,
    IProviderConnectionCredentials credentials) : IMelhorEnvioQuoteClient
{
    public async Task<IReadOnlyList<ShippingServiceQuote>> CalculateAsync(ProviderConnection connection, string originPostalCode,
        string destinationPostalCode, IReadOnlyList<MelhorEnvioQuoteProduct> products, CancellationToken ct)
    {
        var tokens = await credentials.GetTokensAsync(connection.EnterpriseId, connection.Provider, connection.Environment, ct);
        using var request = MelhorEnvioHttp.Request(HttpMethod.Post, connection.Environment, "/api/v2/me/shipment/calculate", options.Value.ContactEmail);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        request.Content = JsonContent.Create(new
        {
            from = new { postal_code = originPostalCode },
            to = new { postal_code = destinationPostalCode },
            products = products.Select(Product => new
            {
                id = Product.VariantId.ToString(CultureInfo.InvariantCulture),
                width = Product.WidthCm,
                height = Product.HeightCm,
                length = Product.LengthCm,
                weight = Product.WeightKg,
                insurance_value = decimal.Round(Product.UnitValue, 2),
                quantity = Product.Quantity
            })
        });

        using var response = await MelhorEnvioHttp.SendAsync(http, request, ct);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new SecurityFailure(409, "Integration.ReauthorizationRequired", "Autorize novamente a conta do Melhor Envio.");

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            throw new SecurityFailure(409, "MelhorEnvio.QuoteRejected", "O Melhor Envio recusou os dados de entrega do carrinho.");

        if (!response.IsSuccessStatusCode)
            throw new SecurityFailure(502, "MelhorEnvio.Unavailable", "A cotação do Melhor Envio está indisponível.");

        JsonElement payload;

        try
        {
            payload = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        }
        catch (JsonException)
        {
            throw InvalidQuoteResponse();
        }

        if (payload.ValueKind != JsonValueKind.Array)
            throw InvalidQuoteResponse();

        return payload.EnumerateArray().Select(ParseQuote).OfType<ShippingServiceQuote>()
            .DistinctBy(Quote => Quote.ServiceId).OrderBy(Quote => Quote.Price).ThenBy(Quote => Quote.ServiceId).ToArray();
    }

    private static ShippingServiceQuote? ParseQuote(JsonElement quote)
    {
        if (Property(quote, "error").ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null) ||
            Text(quote, "currency") is not ("R$" or "BRL"))
            return null;

        var serviceId = Integer(Property(quote, "id"));
        var serviceName = Text(quote, "name");
        var company = Property(quote, "company");
        var companyId = Integer(Property(company, "id"));
        var companyName = Text(company, "name");
        var price = Number(Prefer(quote, "custom_price", "price"));
        var days = Integer(Prefer(quote, "custom_delivery_time", "delivery_time"));
        var packages = Property(quote, "packages");

        if (serviceId is null or <= 0 || companyId is null or <= 0 ||
            string.IsNullOrWhiteSpace(serviceName) || serviceName.Length > 200 ||
            string.IsNullOrWhiteSpace(companyName) || companyName.Length > 200 ||
            price is null or < 0 or > 99999999 || decimal.Round(price.Value, 2) != price.Value ||
            days is null or < 0 || packages.ValueKind != JsonValueKind.Array || packages.GetArrayLength() == 0)
            return null;

        var volumes = new List<ShippingVolume>();

        foreach (var package in packages.EnumerateArray())
        {
            var dimensions = Property(package, "dimensions");
            var weight = Number(Property(package, "weight"));
            var height = Number(Property(dimensions, "height"));
            var width = Number(Property(dimensions, "width"));
            var length = Number(Property(dimensions, "length"));
            var insurance = Number(Property(package, "insurance_value")) ?? 0;

            if (weight is null or <= 0 || height is null or <= 0 || width is null or <= 0 || length is null or <= 0 || insurance < 0)
                return null;

            volumes.Add(new ShippingVolume(weight.Value, height.Value, width.Value, length.Value, insurance));
        }

        return new ShippingServiceQuote(serviceId.Value, serviceName, companyId.Value, companyName, price.Value, days.Value, volumes);
    }

    private static JsonElement Property(JsonElement item, string name)
    {
        return item.ValueKind == JsonValueKind.Object && item.TryGetProperty(name, out var property) ? property : default;
    }

    private static JsonElement Prefer(JsonElement item, string name, string fallback)
    {
        var property = Property(item, name);

        return property.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? Property(item, fallback) : property;
    }

    private static string? Text(JsonElement item, string name)
    {
        var property = Property(item, name);

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static int? Integer(JsonElement value)
    {
        var number = Number(value);

        return number is >= int.MinValue and <= int.MaxValue && decimal.Truncate(number.Value) == number.Value ? (int)number.Value : null;
    }

    private static decimal? Number(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
            JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var number) => number,
            _ => null
        };
    }

    private static SecurityFailure InvalidQuoteResponse()
    {
        return new SecurityFailure(502, "MelhorEnvio.InvalidQuoteResponse", "O Melhor Envio retornou uma cotação inválida.");
    }
}
