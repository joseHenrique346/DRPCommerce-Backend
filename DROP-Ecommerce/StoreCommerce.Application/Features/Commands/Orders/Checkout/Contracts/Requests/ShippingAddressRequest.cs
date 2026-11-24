namespace StoreCommerce.Application.Features.Commands;

public sealed record ShippingAddressRequest(bool UseProfileAddress = false, string? ShippingAddressLine = null,
    string? ShippingCity = null, long? ShippingStateId = null, string? ShippingZipCode = null, long? AddressId = null);
