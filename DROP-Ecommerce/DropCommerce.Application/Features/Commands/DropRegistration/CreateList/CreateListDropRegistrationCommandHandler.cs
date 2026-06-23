using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropRegistrationCommandHandler(IRepository<DropRegistration> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropRegistrationCommand, CreateListDropRegistrationCommand, DropRegistration>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropRegistrationCommand> GetCommandList(CreateListDropRegistrationCommand request) => request.commands;

    protected override DropRegistration CreateEntity(CreateDropRegistrationCommand command) =>
        DropRegistration.Create(command.dropEventId, command.customerId, command.statusId, command.isEligible, command.eligibilityReason, command.registeredAt, command.eligibilityCheckedAt);
}
