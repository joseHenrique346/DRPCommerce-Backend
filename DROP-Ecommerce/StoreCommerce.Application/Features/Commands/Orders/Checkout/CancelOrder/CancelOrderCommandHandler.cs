using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Enums.Orders;
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

public sealed class CancelOrderCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)    : WorkflowRequestHandler<CancelOrderCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(CancelOrderCommand request, CancellationToken ct)
    {
        await lifecycle.CancelAsync(tenant.GetEnterpriseId(), request.Id, request.Reason.Trim(), OrderChangeSource.Backoffice, actor.UserId, null, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, null, ct))!, backoffice: true);
    }
}
