using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListDepartmentCommandHandler(IRepository<Department> repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateDepartmentCommand, CreateListDepartmentCommand, Department>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDepartmentCommand> GetCommandList(CreateListDepartmentCommand request) => request.commands;

    protected override Department CreateEntity(CreateDepartmentCommand command) =>
        Department.Create(tenantProvider.GetEnterpriseId(), command.name, command.description, command.isActive);
}
