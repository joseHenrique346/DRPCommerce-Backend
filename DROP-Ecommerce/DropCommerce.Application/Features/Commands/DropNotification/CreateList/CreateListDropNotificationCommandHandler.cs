using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropNotificationCommandHandler(IRepository<DropNotification> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropNotificationCommand, CreateListDropNotificationCommand, DropNotification>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropNotificationCommand> GetCommandList(CreateListDropNotificationCommand request) => request.commands;

    protected override DropNotification CreateEntity(CreateDropNotificationCommand command) =>
        DropNotification.Create(command.dropEventId, command.customerId, command.channelId, command.typeId, command.subject, command.body, command.statusId, command.scheduledAt, command.sentAt);
}
