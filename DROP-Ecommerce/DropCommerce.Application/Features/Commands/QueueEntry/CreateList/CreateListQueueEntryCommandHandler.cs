using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListQueueEntryCommandHandler(IRepository<QueueEntry> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateQueueEntryCommand, CreateListQueueEntryCommand, QueueEntry>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateQueueEntryCommand> GetCommandList(CreateListQueueEntryCommand request) => request.commands;

    protected override QueueEntry CreateEntity(CreateQueueEntryCommand command) =>
        QueueEntry.Create(command.dropEventId, command.customerId, command.sessionToken, command.position, command.statusId, command.deviceFingerprint, command.ipAddress, command.userAgent, command.enteredAt, command.calledAt, command.expiredAt, command.checkedOutAt);
}
