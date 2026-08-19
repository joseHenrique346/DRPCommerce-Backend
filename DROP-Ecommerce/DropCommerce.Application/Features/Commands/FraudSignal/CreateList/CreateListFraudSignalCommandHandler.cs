using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListFraudSignalCommandHandler(IRepository<FraudSignal> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateFraudSignalCommand, CreateListFraudSignalCommand, FraudSignal>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateFraudSignalCommand> GetCommandList(CreateListFraudSignalCommand request) => request.commands;

    protected override FraudSignal CreateEntity(CreateFraudSignalCommand command) =>
        FraudSignal.Create(command.customerId, command.dropEventId, command.queueEntryId, command.signalTypeId, command.severityId, command.description, command.ipAddress, command.deviceFingerprint, command.isConfirmed, command.wasBlocked, command.detectedAt, command.reviewedAt);
}
