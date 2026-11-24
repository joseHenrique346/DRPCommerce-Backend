using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Orders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Orders;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ShipOrderCommandHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    OrderWorkflowScope scope,
    ICurrentIdentity actor,
    TimeProvider time,
    ITenantProvider tenant)
    : WorkflowRequestHandler<ShipOrderCommand, OrderDetailResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<OrderDetailResponse> ExecuteAsync(ShipOrderCommand command, CancellationToken ct)
    {
        var data = command.Request;
        var details = data.ShipmentTypeId is long typeId
            ? new ShipmentDetails(typeId, data.CarrierName, data.TrackingCode, DateTime.SpecifyKind(data.EstimatedDelivery!.Value, DateTimeKind.Utc))
            : null;
        var enterpriseId = tenant.GetEnterpriseId();
        var orderId = command.Id;
        long? customerId = null;
        var source = OrderChangeSource.Backoffice;
        long? actorId = actor.UserId;
        var note = data.Note;

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);

        var order = await orders.GetForWorkflowAsync(enterpriseId, orderId, ct);

        if (order is null || (customerId.HasValue && order.CustomerId != customerId))
            throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado.");

        using (scope.Begin(enterpriseId, customerId))
        {
            await ApplyAsync();
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(command.Id, null, ct))!, backoffice: true);

        Task ApplyAsync()
        {
            var now = Now;
            var physical = order.PickupAtStore || order.ShippingQuote is not null ||
                order.ListOrderItem.Any(Item => Item.ProductId.HasValue && Item.ProductVariant?.Product.IsDigital != true);

            if (physical && details is null)
                throw new SecurityFailure(400, "Order.ShipmentRequired", "Informe o tipo de envio, a transportadora e a previsão de entrega.");

            if (order.PickupAtStore && details?.ShipmentTypeId != ShipmentType.Pickup.Id ||
                order.ShippingQuote is not null && details?.ShipmentTypeId == ShipmentType.Pickup.Id)
                throw new SecurityFailure(409, "Order.FulfillmentMismatch", "A forma de envio precisa seguir a opção escolhida no checkout.");

            var shipment = details is null ? null : Shipment.CreateForOrder(order, details.ShipmentTypeId, details.CarrierName, details.TrackingCode,
                details.EstimatedDelivery, now);

            order.MarkShipped(shipment, source, actorId, note, now);

            return Task.CompletedTask;
        }
    }
}
