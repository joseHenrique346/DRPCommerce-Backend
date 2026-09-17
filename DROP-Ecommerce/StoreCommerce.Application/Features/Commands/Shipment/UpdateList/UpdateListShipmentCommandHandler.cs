using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListShipmentCommandHandler(IRepository<Shipment> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateShipmentCommand, UpdateListShipmentCommand, Shipment>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateShipmentCommand> GetCommandList(UpdateListShipmentCommand request) => request.commands;

    protected override long GetById(UpdateShipmentCommand command) => command.id;

    protected override void ApplyChanges(Shipment entity, UpdateShipmentCommand command)
    {
        entity.UpdateDetails(command.supplierId, command.typeId, command.carrierName, command.trackingCode, command.shippingCost, command.addressLine, command.city, 0, command.zipCode, command.country, command.estimatedDelivery);
        entity.UpdateStatus(command.statusId);
    }
}
