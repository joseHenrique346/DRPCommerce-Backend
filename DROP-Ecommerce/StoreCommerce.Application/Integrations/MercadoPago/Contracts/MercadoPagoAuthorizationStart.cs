namespace StoreCommerce.Application.Integrations.MercadoPago;

public sealed record MercadoPagoAuthorizationStart(string AuthorizationUrl, string BrowserBinding, DateTime ExpiresAt);
