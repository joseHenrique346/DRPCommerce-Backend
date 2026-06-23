using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListWaitlistEntryCommandHandler(IRepository<WaitlistEntry> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateWaitlistEntryCommand, UpdateListWaitlistEntryCommand, WaitlistEntry>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateWaitlistEntryCommand> GetCommandList(UpdateListWaitlistEntryCommand request) => request.commands;

    protected override long GetById(UpdateWaitlistEntryCommand command) => command.id;

    protected override void ApplyChanges(WaitlistEntry entity, UpdateWaitlistEntryCommand command)
    {
        entity.Update(command.dropEventId, command.dropProductId, command.customerId, command.position, command.statusId, command.notificationSent, command.joinedAt, command.notifiedAt, command.expiresAt);
    }
}
