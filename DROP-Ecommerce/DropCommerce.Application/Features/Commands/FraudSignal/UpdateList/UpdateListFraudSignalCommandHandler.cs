using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListFraudSignalCommandHandler(IRepository<FraudSignal> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateFraudSignalCommand, UpdateListFraudSignalCommand, FraudSignal>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateFraudSignalCommand> GetCommandList(UpdateListFraudSignalCommand request) => request.commands;

    protected override long GetById(UpdateFraudSignalCommand command) => command.id;

    protected override void ApplyChanges(FraudSignal entity, UpdateFraudSignalCommand command)
    {
        entity.Update(command.customerId, command.dropEventId, command.queueEntryId, command.signalTypeId, command.severityId, command.description, command.ipAddress, command.deviceFingerprint, command.isConfirmed, command.wasBlocked, command.detectedAt, command.reviewedAt);
    }
}
