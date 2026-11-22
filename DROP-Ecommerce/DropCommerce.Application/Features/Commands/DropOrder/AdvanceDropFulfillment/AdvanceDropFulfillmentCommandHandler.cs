using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class AdvanceDropFulfillmentCommandHandler(
    IDropCheckoutRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor,
    IMediator mediator)
    : IRequestHandler<AdvanceDropFulfillmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AdvanceDropFulfillmentCommand command, CancellationToken ct)
    {
        long orderId = command.OrderId;
        long statusId = command.StatusId;

        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasPermission("orders.manage"))
            throw new SecurityFailure(403, "DropPurchase.ManageRequired", "Permissão de gestão de pedidos necessária.");

        var tenant = actor.EnterpriseId;
        using (writes.Begin("sales", tenant))
        {
            await using var tx = await persistence.BeginTransactionAsync(ct);
            await persistence.LockCommercialTenantAsync(tenant, ct);
            var order = await persistence.FindOrderAsync(orderId, tenant, ct)
                ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido não encontrado.");

            if (order.DropOrderPaymentStatusId != 2 || order.FinancialReviewCode is not null || statusId < order.DropOrderStatusId || statusId is < 3 or > 5)
                throw new SecurityFailure(409, "DropPurchase.InvalidTransition", "Transição de entrega inválida.");

            order.AdvanceFulfillment(statusId);
            await unitOfWork.CommitAsync(ct);
            await tx.CommitAsync(ct);
        }

        await mediator.SendRequiredAsync(new AcknowledgeDropFulfillmentCommand(tenant, orderId), ct);

        return Result<bool>.Success(true);
    }
}
