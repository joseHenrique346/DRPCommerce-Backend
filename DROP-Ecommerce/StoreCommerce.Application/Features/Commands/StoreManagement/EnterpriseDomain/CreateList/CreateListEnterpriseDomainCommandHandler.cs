using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEnterpriseDomainCommandHandler(IEnterpriseDomainRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateEnterpriseDomainCommand, CreateListEnterpriseDomainCommand, EnterpriseDomain>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateEnterpriseDomainCommand> GetCommandList(CreateListEnterpriseDomainCommand request) => request.commands;

    protected override EnterpriseDomain CreateEntity(CreateEnterpriseDomainCommand command) =>
        EnterpriseDomain.Create(tenantProvider.GetEnterpriseId(), command.host, command.isPrimary, command.isActive);
}
