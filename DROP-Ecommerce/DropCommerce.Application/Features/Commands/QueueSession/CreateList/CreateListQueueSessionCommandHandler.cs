using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListQueueSessionCommandHandler(IRepository<QueueSession> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateQueueSessionCommand, CreateListQueueSessionCommand, QueueSession>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateQueueSessionCommand> GetCommandList(CreateListQueueSessionCommand request) => request.commands;

    protected override QueueSession CreateEntity(CreateQueueSessionCommand command) =>
        QueueSession.Create(command.queueEntryId, command.customerId, command.token, command.statusId, command.issuedAt, command.expiresAt, command.lastHeartbeatAt);
}
