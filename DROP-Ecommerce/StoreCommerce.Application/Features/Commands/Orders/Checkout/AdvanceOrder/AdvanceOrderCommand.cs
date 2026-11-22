using StoreCommerce.Application.Enums.Orders;
using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record AdvanceOrderCommand(long Id, OrderStep Step, string? Note) : IRequest<Result<OrderDetailResponse>>;
