using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Orders;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StartOrderProcessingCommandHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    OrderWorkflowScope scope,
    ICurrentIdentity actor,
    TimeProvider time,
    ITenantProvider tenant)
    : WorkflowRequestHandler<StartOrderProcessingCommand, OrderDetailResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<OrderDetailResponse> ExecuteAsync(StartOrderProcessingCommand command, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var orderId = command.Id;
        long? customerId = null;
        var source = OrderChangeSource.Backoffice;
        long? actorId = actor.UserId;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);

        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct);

        if (order is null || (customerId.HasValue && order.CustomerId != customerId))
            throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        using (scope.Begin(enterpriseId, customerId))
        {
            order.StartProcessing(source, actorId, Now);
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(command.Id, null, ct))!, backoffice: true);
    }
}
