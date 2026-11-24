using System.Text.Json.Serialization;

namespace SharedCommerce.Application.Contracts.Drop;

public sealed record StoreProductResponse(
    [property: JsonPropertyName("productId")] long ProductId,
    [property: JsonPropertyName("enterpriseId")] long EnterpriseId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sku")] string SKU,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("price")] decimal Price = 0);
