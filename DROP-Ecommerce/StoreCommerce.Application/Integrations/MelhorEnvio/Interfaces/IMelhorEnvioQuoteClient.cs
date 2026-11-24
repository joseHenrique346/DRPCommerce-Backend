using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Integrations.MelhorEnvio;

public interface IMelhorEnvioQuoteClient
{
    Task<IReadOnlyList<ShippingServiceQuote>> CalculateAsync(ProviderConnection connection, string originPostalCode,
        string destinationPostalCode, IReadOnlyList<MelhorEnvioQuoteProduct> products, CancellationToken ct);
}
