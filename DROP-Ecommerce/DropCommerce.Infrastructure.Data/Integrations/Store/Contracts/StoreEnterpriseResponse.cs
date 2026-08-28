using System.Text.Json.Serialization;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;

public sealed record StoreEnterpriseResponse(
    [property: JsonPropertyName("enterpriseId")] long EnterpriseId,
    [property: JsonPropertyName("tradeName")] string TradeName,
    [property: JsonPropertyName("isActive")] bool IsActive);
