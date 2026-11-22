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

public sealed class AdvanceOrderCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)    : WorkflowRequestHandler<AdvanceOrderCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(AdvanceOrderCommand request, CancellationToken ct)
    {
        await lifecycle.AdvanceAsync(tenant.GetEnterpriseId(), request.Id, request.Step, OrderChangeSource.Backoffice, actor.UserId, request.Note, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, null, ct))!, backoffice: true);
    }
}
