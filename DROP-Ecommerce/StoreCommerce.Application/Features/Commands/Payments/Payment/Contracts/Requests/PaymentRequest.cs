using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

/// <param name="IdempotencyKey">Chave estável por pedido, para o gateway não cobrar duas vezes.</param>
public sealed record PaymentRequest(long EnterpriseId, long OrderId, long CustomerId, string CustomerName, string CustomerEmail, decimal Amount, string CurrencyCode, long TransactionMethodId, DateTime ExpiresAt, string IdempotencyKey, PaymentProviderContext Context, string ExternalReference, string? ReturnFlow = null);
