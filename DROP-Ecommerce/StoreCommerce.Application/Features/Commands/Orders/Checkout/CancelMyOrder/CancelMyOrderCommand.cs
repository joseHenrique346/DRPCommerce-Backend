using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// O cliente só cancela pedidos que ainda aguardam pagamento.
/// </summary>
public sealed record CancelMyOrderCommand(long Id, string? Reason) : IRequest<Result<OrderDetailResponse>>;
