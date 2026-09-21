using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListRoleCommandHandler(IRepository<Role> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateRoleCommand, UpdateListRoleCommand, Role>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateRoleCommand> GetCommandList(UpdateListRoleCommand request) => request.commands;

    protected override long GetById(UpdateRoleCommand command) => command.id;

    protected override void ApplyChanges(Role entity, UpdateRoleCommand command)
    {
        entity.UpdateDetails(command.name, command.description);
    }
}
