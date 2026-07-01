using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListQueueEntryCommandHandler(IRepository<QueueEntry> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateQueueEntryCommand, UpdateListQueueEntryCommand, QueueEntry>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateQueueEntryCommand> GetCommandList(UpdateListQueueEntryCommand request) => request.commands;

    protected override long GetById(UpdateQueueEntryCommand command) => command.id;

    protected override void ApplyChanges(QueueEntry entity, UpdateQueueEntryCommand command)
    {
        entity.Update(command.dropEventId, command.customerId, command.sessionToken, command.position, command.statusId, command.deviceFingerprint, command.ipAddress, command.userAgent, command.enteredAt, command.calledAt, command.expiredAt, command.checkedOutAt);
    }
}
