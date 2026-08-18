using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListQueueSessionCommandHandler(IRepository<QueueSession> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateQueueSessionCommand, UpdateListQueueSessionCommand, QueueSession>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateQueueSessionCommand> GetCommandList(UpdateListQueueSessionCommand request) => request.commands;

    protected override long GetById(UpdateQueueSessionCommand command) => command.id;

    protected override void ApplyChanges(QueueSession entity, UpdateQueueSessionCommand command)
    {
        entity.Update(command.queueEntryId, command.customerId, command.token, command.statusId, command.issuedAt, command.expiresAt, command.lastHeartbeatAt);
    }
}
