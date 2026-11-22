using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

/// <summary>
/// Cobrança criada. CheckoutUrl (cartão/redirecionamento) e Instructions (código Pix, linha digitável) são exibidos ao cliente.
/// </summary>
public sealed record PaymentSession(string Reference, string? CheckoutUrl, string? Instructions, DateTime? ExpiresAt, string? PreferenceId = null, string? PaymentId = null);
