using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListWaitlistEntryCommandHandler(IRepository<WaitlistEntry> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateWaitlistEntryCommand, CreateListWaitlistEntryCommand, WaitlistEntry>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateWaitlistEntryCommand> GetCommandList(CreateListWaitlistEntryCommand request) => request.commands;

    protected override WaitlistEntry CreateEntity(CreateWaitlistEntryCommand command) =>
        WaitlistEntry.Create(command.dropEventId, command.dropProductId, command.customerId, command.position, command.statusId, command.notificationSent, command.joinedAt, command.notifiedAt, command.expiresAt);
}
