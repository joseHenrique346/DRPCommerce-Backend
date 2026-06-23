using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropReservationCommandHandler(IRepository<DropReservation> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropReservationCommand, UpdateListDropReservationCommand, DropReservation>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropReservationCommand> GetCommandList(UpdateListDropReservationCommand request) => request.commands;

    protected override long GetById(UpdateDropReservationCommand command) => command.id;

    protected override void ApplyChanges(DropReservation entity, UpdateDropReservationCommand command)
    {
        entity.Update(command.dropEventId, command.dropProductId, command.customerId, command.queueEntryId, command.statusId, command.quantity, command.unitPrice, command.totalAmount, command.lockToken, command.reservedAt, command.expiresAt, command.confirmedAt, command.cancelledAt);
    }
}
