using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

// ---- Backoffice

public sealed record GetOrderDetailQuery(long Id) : IRequest<Result<OrderDetailResponse>>;
