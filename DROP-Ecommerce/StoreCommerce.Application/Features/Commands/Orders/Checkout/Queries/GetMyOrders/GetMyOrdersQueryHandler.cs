using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Orders;
using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Orders;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetMyOrdersQueryHandler(IOrderRepository orders, ICurrentIdentity actor)    : WorkflowRequestHandler<GetMyOrdersQuery, PagedResponse<OrderSummaryResponse>>
{
    protected override async Task<PagedResponse<OrderSummaryResponse>> ExecuteAsync(GetMyOrdersQuery request, CancellationToken ct)
    {
        var (items, count) = await orders.GetPageForCustomerAsync(actor.CustomerId, request.Page, request.PageSize, ct);

        return new(items.Select(OrderSummaryResponse.FromEntity).ToList(), count, request.Page, request.PageSize);
    }
}
