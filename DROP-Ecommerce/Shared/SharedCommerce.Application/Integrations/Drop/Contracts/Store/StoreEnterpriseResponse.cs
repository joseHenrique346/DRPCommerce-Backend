using System.Text.Json.Serialization;

namespace SharedCommerce.Application.Contracts.Drop;

public sealed record StoreEnterpriseResponse(
    [property: JsonPropertyName("enterpriseId")] long EnterpriseId,
    [property: JsonPropertyName("tradeName")] string TradeName,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("legalName")] string? LegalName = null);
