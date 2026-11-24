using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEnterpriseMembershipCommandHandler(IEnterpriseMembershipRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateEnterpriseMembershipCommand, CreateListEnterpriseMembershipCommand, EnterpriseMembership>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateEnterpriseMembershipCommand> GetCommandList(CreateListEnterpriseMembershipCommand request) => request.commands;

    protected override EnterpriseMembership CreateEntity(CreateEnterpriseMembershipCommand command) =>
        EnterpriseMembership.Create(tenantProvider.GetEnterpriseId(), command.userId, command.kindId, command.isActive, command.joinedAt);
}
