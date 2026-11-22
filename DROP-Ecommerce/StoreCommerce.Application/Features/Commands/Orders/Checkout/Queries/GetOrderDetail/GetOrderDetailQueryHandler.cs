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

public sealed class GetOrderDetailQueryHandler(IOrderRepository orders) : WorkflowRequestHandler<GetOrderDetailQuery, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(GetOrderDetailQuery request, CancellationToken ct) =>
        OrderDetailResponse.FromEntity(await orders.GetDetailAsync(request.Id, null, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado."), backoffice: true);
}
