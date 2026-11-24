namespace StoreCommerce.Application.Features.Commands;

public sealed record ProviderAuthorizationStart(string AuthorizationUrl, string BrowserBinding, DateTime ExpiresAt);
