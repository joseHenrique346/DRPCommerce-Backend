using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEnterpriseDomainCommandHandler(IEnterpriseDomainRepository repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateEnterpriseDomainCommand, CreateListEnterpriseDomainCommand, EnterpriseDomain>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateEnterpriseDomainCommand> GetCommandList(CreateListEnterpriseDomainCommand request) => request.commands;

    protected override EnterpriseDomain CreateEntity(CreateEnterpriseDomainCommand command) =>
        EnterpriseDomain.Create(command.enterpriseId, command.host, command.isPrimary, command.isActive);
}
