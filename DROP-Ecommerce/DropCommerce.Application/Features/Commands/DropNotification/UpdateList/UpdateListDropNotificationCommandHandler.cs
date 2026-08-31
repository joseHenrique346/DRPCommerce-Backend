using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropNotificationCommandHandler(IRepository<DropNotification> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropNotificationCommand, UpdateListDropNotificationCommand, DropNotification>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropNotificationCommand> GetCommandList(UpdateListDropNotificationCommand request) => request.commands;

    protected override long GetById(UpdateDropNotificationCommand command) => command.id;

    protected override void ApplyChanges(DropNotification entity, UpdateDropNotificationCommand command)
    {
        entity.Update(command.dropEventId, command.customerId, command.channelId, command.typeId, command.subject, command.body, command.statusId, command.scheduledAt, command.sentAt);
    }
}
