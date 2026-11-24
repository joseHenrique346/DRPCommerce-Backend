namespace StoreCommerce.Application.Features.Commands;

public sealed record CheckoutShippingOptionResponse(int ServiceId, string ServiceName, string CarrierName,
    decimal Price, int DeliveryDays, string QuoteToken);
