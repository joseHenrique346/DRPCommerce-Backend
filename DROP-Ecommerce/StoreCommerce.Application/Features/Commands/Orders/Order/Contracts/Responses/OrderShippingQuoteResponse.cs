namespace StoreCommerce.Application.Contracts.Order;

public sealed record OrderShippingQuoteResponse(string Provider, int ServiceId, string ServiceName, string CarrierName,
    int DeliveryDays, int VolumeCount);
