namespace StoreCommerce.Domain.Entity;

public sealed record ShippingServiceQuote(
    int ServiceId, string ServiceName, int CarrierId, string CarrierName,
    decimal Price, int DeliveryDays, IReadOnlyList<ShippingVolume> Volumes);
