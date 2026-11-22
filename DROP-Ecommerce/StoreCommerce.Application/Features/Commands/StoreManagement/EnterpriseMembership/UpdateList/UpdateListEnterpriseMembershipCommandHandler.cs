using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEnterpriseMembershipCommandHandler(IEnterpriseMembershipRepository repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateEnterpriseMembershipCommand, UpdateListEnterpriseMembershipCommand, EnterpriseMembership>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateEnterpriseMembershipCommand> GetCommandList(UpdateListEnterpriseMembershipCommand request) => request.commands;

    protected override long GetById(UpdateEnterpriseMembershipCommand command) => command.id;

    protected override void ApplyChanges(EnterpriseMembership entity, UpdateEnterpriseMembershipCommand command) =>
        entity.UpdateDetails(command.kindId, command.isActive, command.joinedAt);
}
