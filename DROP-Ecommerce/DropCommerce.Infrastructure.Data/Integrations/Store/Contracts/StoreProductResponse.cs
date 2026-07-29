using System.Text.Json.Serialization;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;

public sealed record StoreProductResponse(
    [property: JsonPropertyName("productId")] long ProductId,
    [property: JsonPropertyName("enterpriseId")] long EnterpriseId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sku")] string SKU,
    [property: JsonPropertyName("isActive")] bool IsActive);
