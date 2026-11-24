using System.Text.Json.Serialization;

namespace SharedCommerce.Application.Contracts.Drop;

public sealed record StoreCustomerResponse(
    [property: JsonPropertyName("customerId")] long CustomerId,
    [property: JsonPropertyName("enterpriseId")] long EnterpriseId,
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("isActive")] bool IsActive);
