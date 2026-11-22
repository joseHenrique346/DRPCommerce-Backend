using StoreCommerce.Application.Enums.Payments;
using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

// ---- Gateway de pagamento

/// <summary>
/// Resultado de pagamento informado pelo gateway. Deve ser enviado pelo endpoint de webhook do gateway real, depois de validar
/// a assinatura da notificação e de definir a loja da cobrança como tenant confiável da requisição.
/// </summary>
public sealed record ApplyPaymentResultCommand(string Provider, string Reference, PaymentResultStatus Status, decimal? PaidAmount)
    : IRequest<Result<OrderDetailResponse>>;
