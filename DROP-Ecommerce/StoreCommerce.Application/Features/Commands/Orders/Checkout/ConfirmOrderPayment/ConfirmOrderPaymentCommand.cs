using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Registra manualmente o pagamento recebido. Usa o mesmo fluxo da aprovação vinda do gateway.
/// </summary>
public sealed record ConfirmOrderPaymentCommand(long Id, long? TransactionMethodId, string? GatewayReference, string? Note)
    : IRequest<Result<OrderDetailResponse>>;
