using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetMyOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResponse<OrderSummaryResponse>>>;
