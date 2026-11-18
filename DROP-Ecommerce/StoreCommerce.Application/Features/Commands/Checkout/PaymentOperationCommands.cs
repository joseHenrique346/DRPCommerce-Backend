using MediatR;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Payments;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ResolvePaymentOperationCommand(long OrderId, long OperationId, string? Reference, string? Note,
    bool Retry) : IRequest<Result<OrderDetailResponse>>;

public sealed class ResolvePaymentOperationCommandHandler(IPaymentOperationStore operations, IOrderRepository orders,
    ICurrentIdentity actor) : WorkflowRequestHandler<ResolvePaymentOperationCommand, OrderDetailResponse>
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
