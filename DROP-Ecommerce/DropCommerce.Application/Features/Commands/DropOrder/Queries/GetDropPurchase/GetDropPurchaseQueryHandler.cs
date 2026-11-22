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

public sealed class GetDropPurchaseQueryHandler(
    IDropCheckoutRepository persistence,
    ICurrentDropActor actor)
    : IRequestHandler<GetDropPurchaseQuery, Result<DropPurchaseResponse>>
{
    private long Customer => actor.CustomerId;

    public async Task<Result<DropPurchaseResponse>> Handle(GetDropPurchaseQuery command, CancellationToken ct)
    {
        long orderId = command.OrderId;

        var tenant = actor.EnterpriseId;
        var customer = Customer;
        var order = await persistence.FindCustomerOrderAsync(orderId, customer, tenant, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido não encontrado.");

        return Result<DropPurchaseResponse>.Success(new DropPurchaseResponse(
            order.Id, order.DropEventId, order.DropReservationId, order.DropOrderStatusId, order.DropOrderPaymentStatusId,
            order.TotalAmount, order.BuyerNameSnapshot, order.BuyerEmailSnapshot, order.PaymentCheckoutUrl, order.FinancialReviewCode,
            order.ShippingAddressLine, order.ShippingCity, order.ShippingState, order.ShippingZipCode,
            order.ListDropOrderItem.Select(Item => new DropPurchaseItemResponse(
                Item.ItemName, Item.SKU, Item.Quantity, Item.UnitPrice, Item.TotalPrice, Item.OptionsSnapshotJson))));
    }
}
