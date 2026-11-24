namespace StoreCommerce.Application.Contracts;

public sealed record PlatformWebhookBody(string? Type, PlatformWebhookData? Data);
