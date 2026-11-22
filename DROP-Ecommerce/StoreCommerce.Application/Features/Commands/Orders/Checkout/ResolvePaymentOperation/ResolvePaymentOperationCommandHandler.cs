using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Interfaces.Orders;
using MediatR;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ResolvePaymentOperationCommandHandler(IPaymentOperationStore operations, IOrderRepository orders, ICurrentIdentity actor) : WorkflowRequestHandler<ResolvePaymentOperationCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(ResolvePaymentOperationCommand request, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.OrdersCancel);
        actor.RequirePermission(PermissionCodes.OrdersRefund);
        await operations.ResolveAsync(actor.EnterpriseId, request.OrderId, request.OperationId, actor.UserId,
            request.Reference?.Trim(), request.Note?.Trim(), request.Retry, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.OrderId, null, ct))!, backoffice: true);
    }
}
