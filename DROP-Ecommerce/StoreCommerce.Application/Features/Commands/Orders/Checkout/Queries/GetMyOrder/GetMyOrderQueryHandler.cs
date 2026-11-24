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

public sealed class GetMyOrderQueryHandler(IOrderRepository orders, ICurrentIdentity actor) : WorkflowRequestHandler<GetMyOrderQuery, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(GetMyOrderQuery request, CancellationToken ct) =>
        OrderDetailResponse.FromEntity(await orders.GetDetailAsync(request.Id, actor.CustomerId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado."), backoffice: false);
}
