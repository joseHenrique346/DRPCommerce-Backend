using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

// ---- Vitrine (cliente autenticado)

public sealed record PlaceOrderCommand(CheckoutRequest Request) : IRequest<Result<OrderDetailResponse>>;

public sealed record GetMyOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResponse<OrderSummaryResponse>>>;

public sealed record GetMyOrderQuery(long Id) : IRequest<Result<OrderDetailResponse>>;

/// <summary>
/// O cliente só cancela pedidos que ainda aguardam pagamento.
/// </summary>
public sealed record CancelMyOrderCommand(long Id, string? Reason) : IRequest<Result<OrderDetailResponse>>;

// ---- Backoffice

public sealed record GetOrderDetailQuery(long Id) : IRequest<Result<OrderDetailResponse>>;

/// <summary>
/// Registra manualmente o pagamento recebido. Usa o mesmo fluxo da aprovação vinda do gateway.
/// </summary>
public sealed record ConfirmOrderPaymentCommand(long Id, long? TransactionMethodId, string? GatewayReference, string? Note)
    : IRequest<Result<OrderDetailResponse>>;

public sealed record AdvanceOrderCommand(long Id, OrderStep Step, string? Note) : IRequest<Result<OrderDetailResponse>>;

public sealed record CancelOrderCommand(long Id, string Reason) : IRequest<Result<OrderDetailResponse>>;

// ---- Gateway de pagamento

/// <summary>
/// Resultado de pagamento informado pelo gateway. Deve ser enviado pelo endpoint de webhook do gateway real, depois de validar
/// a assinatura da notificação e de definir a loja da cobrança como tenant confiável da requisição.
/// </summary>
public sealed record ApplyPaymentResultCommand(string Provider, string Reference, PaymentResultStatus Status, decimal? PaidAmount)
    : IRequest<Result<OrderDetailResponse>>;
