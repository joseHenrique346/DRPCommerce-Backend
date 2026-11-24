namespace SharedCommerce.Application.Contracts.Drop;

public sealed record DropBuyerProfile(long CustomerId, long EnterpriseId, string Name, string Email,
    string AddressLine, string City, string State, string ZipCode);
