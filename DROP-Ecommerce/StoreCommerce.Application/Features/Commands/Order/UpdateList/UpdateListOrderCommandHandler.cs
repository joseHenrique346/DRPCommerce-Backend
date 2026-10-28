using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListOrderCommandHandler(
    IOrderRepository repository,
    IRepository<Coupon> couponRepository,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<UpdateListOrderCommand, Result<List<Order>>>
{
    public async Task<Result<List<Order>>> Handle(UpdateListOrderCommand request, CancellationToken cancellationToken)
    {
        var listId = request.commands.Select(command => command.id).ToList();

        if (listId.Count != listId.Distinct().Count())
            return Result<List<Order>>.Validation($"{nameof(UpdateListOrderCommand)}.DuplicateIds", "A lista contém ids duplicados.");

        var orders = (await repository.GetListWithItemsAsync(listId, cancellationToken)).ToDictionary(order => order.Id);

        var missingId = listId.FirstOrDefault(id => !orders.ContainsKey(id));
        if (missingId > 0)
            return Result<List<Order>>.NotFound("Order.NotFound", $"Order com id {missingId} não encontrado.");

        var now = time.GetUtcNow().UtcDateTime;
        var changedCouponIds = request.commands
            .Where(command => command.couponId != orders[command.id].CouponId)
            .Select(command => command.couponId);
        var coupons = await OrderCouponLoader.LoadAsync(couponRepository, changedCouponIds, cancellationToken);
        var result = new List<Order>(request.commands.Count);

        foreach (var command in request.commands)
        {
            var order = orders[command.id];

            order.UpdateShippingAddress(command.shippingAddressLine, command.shippingCity, command.shippingStateId, command.shippingZipCode, command.notes);

            if (command.couponId != order.CouponId)
            {
                if (!order.IsEditable)
                    return Result<List<Order>>.Validation("Order.NotEditable", $"O cupom do pedido {order.Id} só pode ser alterado enquanto ele estiver pendente.");

                if (command.couponId is long couponId)
                {
                    var error = OrderCouponLoader.Validate(coupons, couponId, now);

                    if (error is not null)
                        return Result<List<Order>>.Failure(error);

                    order.ApplyCoupon(coupons[couponId], now);
                }
                else
                {
                    order.ApplyCoupon(null, now);
                }
            }

            // O status é aplicado por último para que a troca de cupom ocorra enquanto o pedido ainda está pendente.
            order.UpdateStatus(command.statusId, command.paymentStatusId);
            result.Add(order);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<List<Order>>.Success(result);
    }
}
