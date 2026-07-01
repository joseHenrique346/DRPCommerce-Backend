using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropRegistrationCommandHandler(IRepository<DropRegistration> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropRegistrationCommand, UpdateListDropRegistrationCommand, DropRegistration>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropRegistrationCommand> GetCommandList(UpdateListDropRegistrationCommand request) => request.commands;

    protected override long GetById(UpdateDropRegistrationCommand command) => command.id;

    protected override void ApplyChanges(DropRegistration entity, UpdateDropRegistrationCommand command)
    {
        entity.Update(command.dropEventId, command.customerId, command.statusId, command.isEligible, command.eligibilityReason, command.registeredAt, command.eligibilityCheckedAt);
    }
}
