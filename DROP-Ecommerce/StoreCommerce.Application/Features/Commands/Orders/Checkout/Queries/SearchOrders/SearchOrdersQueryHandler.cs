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

/// <summary>
/// Busca de pedidos do backoffice usando a identificação histórica do comprador, independente do cadastro atual.
/// </summary>
public sealed class SearchOrdersQueryHandler(IOrderRepository orders)    : WorkflowRequestHandler<SearchOrdersQuery, PagedResponse<OrderSearchItemResponse>>
{
    protected override async Task<PagedResponse<OrderSearchItemResponse>> ExecuteAsync(SearchOrdersQuery request, CancellationToken ct)
    {
        var (items, count) = await orders.SearchAsync(new Domain.Models.OrderSearchCriteria(
            request.Number, request.StatusIds, request.PaymentStatusIds, request.CustomerId, request.CouponId,
            request.CreatedFrom, request.CreatedTo, request.MinTotal, request.MaxTotal,
            request.Sort ?? "newest", request.Page, request.PageSize), ct);

        return new(items.Select(Order => OrderSearchItemResponse.FromEntity(Order, Order.BuyerName)).ToList(),
            count, request.Page, request.PageSize);
    }
}
