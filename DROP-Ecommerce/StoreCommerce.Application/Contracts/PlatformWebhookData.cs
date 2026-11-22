using System.Text.Json;

namespace StoreCommerce.Application.Contracts;

public sealed record PlatformWebhookData(JsonElement Id);
