using System.Text.Json.Serialization;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;

public sealed record StoreApiResponse<T>(
    [property: JsonPropertyName("isSuccess")] bool IsSuccess,
    [property: JsonPropertyName("content")] T? Content,
    [property: JsonPropertyName("listMessageErrors")] IReadOnlyCollection<string>? ListMessageErrors);
