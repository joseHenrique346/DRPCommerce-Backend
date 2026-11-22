using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

/// <summary>
/// Confirmação manual de pagamento recebido. Sem integração com gateway, o backoffice registra aqui o pagamento.
/// TransactionMethodId só é exigido em pedidos criados pelo backoffice (sem cobrança registrada).
/// </summary>
public sealed record ConfirmPaymentRequest(long? TransactionMethodId, string? GatewayReference, string? Note);
