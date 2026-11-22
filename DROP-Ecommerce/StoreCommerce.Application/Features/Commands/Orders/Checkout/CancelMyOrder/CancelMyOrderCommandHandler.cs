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

public sealed class CancelMyOrderCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)    : WorkflowRequestHandler<CancelMyOrderCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(CancelMyOrderCommand request, CancellationToken ct)
    {
        var customerId = actor.CustomerId;
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Cancelado pelo cliente" : request.Reason.Trim();
        await lifecycle.CancelAsync(tenant.GetEnterpriseId(), request.Id, reason, OrderChangeSource.Customer, customerId, customerId, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, customerId, ct))!, backoffice: false);
    }
}
