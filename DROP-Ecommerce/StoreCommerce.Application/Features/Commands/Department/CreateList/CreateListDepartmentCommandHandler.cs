using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListDepartmentCommandHandler(IRepository<Department> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDepartmentCommand, CreateListDepartmentCommand, Department>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDepartmentCommand> GetCommandList(CreateListDepartmentCommand request) => request.commands;

    protected override Department CreateEntity(CreateDepartmentCommand command) =>
        Department.Create(command.name, command.description);
}
