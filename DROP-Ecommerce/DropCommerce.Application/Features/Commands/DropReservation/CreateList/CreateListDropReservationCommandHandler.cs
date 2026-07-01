using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropReservationCommandHandler(IRepository<DropReservation> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropReservationCommand, CreateListDropReservationCommand, DropReservation>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropReservationCommand> GetCommandList(CreateListDropReservationCommand request) => request.commands;

    protected override DropReservation CreateEntity(CreateDropReservationCommand command) =>
        DropReservation.Create(command.dropEventId, command.dropProductId, command.customerId, command.queueEntryId, command.statusId, command.quantity, command.unitPrice, command.totalAmount, command.lockToken, command.reservedAt, command.expiresAt, command.confirmedAt, command.cancelledAt);
}
