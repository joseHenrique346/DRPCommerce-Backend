using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEnterpriseDomainCommandHandler(IEnterpriseDomainRepository repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateEnterpriseDomainCommand, UpdateListEnterpriseDomainCommand, EnterpriseDomain>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateEnterpriseDomainCommand> GetCommandList(UpdateListEnterpriseDomainCommand request) => request.commands;

    protected override long GetById(UpdateEnterpriseDomainCommand command) => command.id;

    protected override void ApplyChanges(EnterpriseDomain entity, UpdateEnterpriseDomainCommand command) =>
        entity.UpdateDetails(command.host, command.isPrimary, command.isActive);
}
