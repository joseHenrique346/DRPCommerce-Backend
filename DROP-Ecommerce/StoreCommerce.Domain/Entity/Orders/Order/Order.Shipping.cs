using StoreCommerce.Domain.Integrations;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public partial class Order
{
    public bool PickupAtStore { get; private set; }

    [JsonIgnore]
    public OrderShippingSnapshot? ShippingQuote { get; private set; }

    public void SelectPickup()
    {
        EnsureEditable();

        if (ShippingQuote is not null)
            throw new InvalidOperationException("O pedido já possui uma cotação de entrega.");

        PickupAtStore = true;
    }

    public void ApplyShippingQuote(OrderShippingSnapshot snapshot, StoreSettings settings)
    {
        EnsureEditable();
        EnsureSettings(settings);

        if (PickupAtStore || ShippingQuote is not null)
            throw new InvalidOperationException("O pedido já possui uma forma de entrega.");

        if (snapshot.Provider != IntegrationProviders.MelhorEnvio || snapshot.ConnectionId <= 0 ||
            snapshot.Service.Price < 0 || decimal.Round(snapshot.Service.Price, 2) != snapshot.Service.Price ||
            snapshot.Service.Volumes.Count == 0 || settings.CurrencyCode != StoreSettings.DefaultCurrencyCode)
            throw new ArgumentException("Cotação inválida para este pedido.", nameof(snapshot));

        ShippingQuote = snapshot;
        ShippingCost = snapshot.Service.Price;
        Recalculate(settings);
    }
}
