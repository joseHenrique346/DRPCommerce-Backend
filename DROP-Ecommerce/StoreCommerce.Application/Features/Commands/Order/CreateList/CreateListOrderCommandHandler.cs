using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListOrderCommandHandler(IRepository<Order> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateOrderCommand, CreateListOrderCommand, Order>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateOrderCommand> GetCommandList(CreateListOrderCommand request) => request.commands;

    protected override Order CreateEntity(CreateOrderCommand command) =>
        Order.Create(command.enterpriseId, command.customerId, command.couponId, command.statusId, command.paymentStatusId, command.subTotal, command.discountAmount, command.shippingCost, command.taxAmount, command.totalAmount, command.shippingAddressLine, command.shippingCity, 0, command.shippingZipCode, command.notes);
}
