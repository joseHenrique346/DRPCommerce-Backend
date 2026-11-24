using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

/// <summary>
/// Cobrança do pedido. CheckoutUrl e Instructions vêm do gateway (link de pagamento, código Pix, linha digitável).
/// </summary>
public sealed record PaymentInfoResponse(string Provider, long MethodId, string Method, long StatusId, string Status, decimal Amount, string Reference, string? CheckoutUrl, string? Instructions, DateTime? ExpiresAt, DateTime? PaidAt, DateTime? RefundedAt, PaymentAttemptInfoResponse? Attempt = null);
