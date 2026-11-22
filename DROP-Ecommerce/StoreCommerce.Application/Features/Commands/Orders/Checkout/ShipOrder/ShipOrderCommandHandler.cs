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

public sealed class ShipOrderCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)    : WorkflowRequestHandler<ShipOrderCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(ShipOrderCommand request, CancellationToken ct)
    {
        var data = request.Request;
        var details = data.ShipmentTypeId is long typeId
            ? new ShipmentDetails(typeId, data.CarrierName, data.TrackingCode, DateTime.SpecifyKind(data.EstimatedDelivery!.Value, DateTimeKind.Utc))
            : null;

        await lifecycle.ShipAsync(tenant.GetEnterpriseId(), request.Id, details, OrderChangeSource.Backoffice, actor.UserId, data.Note, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, null, ct))!, backoffice: true);
    }
}
