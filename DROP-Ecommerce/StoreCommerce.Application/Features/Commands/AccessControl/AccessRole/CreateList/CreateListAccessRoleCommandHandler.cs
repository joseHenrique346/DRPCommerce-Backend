using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListAccessRoleCommandHandler(IAccessRoleRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateAccessRoleCommand, CreateListAccessRoleCommand, AccessRole>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateAccessRoleCommand> GetCommandList(CreateListAccessRoleCommand request) => request.commands;

    protected override AccessRole CreateEntity(CreateAccessRoleCommand command) =>
        AccessRole.Create(tenantProvider.GetEnterpriseId(), command.name, command.description, command.isActive);
}
