namespace StoreCommerce.Application.Features.Commands;

public sealed record CheckoutDeliveryAddress(string AddressLine, string City, long StateId, string ZipCode);
