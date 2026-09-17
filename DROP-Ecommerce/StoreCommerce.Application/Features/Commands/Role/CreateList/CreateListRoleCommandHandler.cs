using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListRoleCommandHandler(IRepository<Role> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateRoleCommand, CreateListRoleCommand, Role>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateRoleCommand> GetCommandList(CreateListRoleCommand request) => request.commands;

    protected override Role CreateEntity(CreateRoleCommand command) =>
        Role.Create(command.name, command.description);
}
