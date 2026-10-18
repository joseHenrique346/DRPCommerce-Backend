using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListAccessRoleCommandHandler(IAccessRoleRepository repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateAccessRoleCommand, UpdateListAccessRoleCommand, AccessRole>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateAccessRoleCommand> GetCommandList(UpdateListAccessRoleCommand request) => request.commands;

    protected override long GetById(UpdateAccessRoleCommand command) => command.id;

    protected override void ApplyChanges(AccessRole entity, UpdateAccessRoleCommand command) =>
        entity.UpdateDetails(command.name, command.description, command.isActive);
}
