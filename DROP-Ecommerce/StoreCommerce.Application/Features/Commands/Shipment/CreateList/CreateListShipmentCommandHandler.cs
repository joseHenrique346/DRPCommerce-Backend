using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListShipmentCommandHandler(IRepository<Shipment> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateShipmentCommand, CreateListShipmentCommand, Shipment>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateShipmentCommand> GetCommandList(CreateListShipmentCommand request) => request.commands;

    protected override Shipment CreateEntity(CreateShipmentCommand command) =>
        Shipment.Create(command.orderId, command.supplierId, command.typeId, command.carrierName, command.trackingCode, command.statusId, command.shippingCost, command.addressLine, command.city, 0, command.zipCode, command.country, command.estimatedDelivery, command.shippedAt, command.deliveredAt);
}
