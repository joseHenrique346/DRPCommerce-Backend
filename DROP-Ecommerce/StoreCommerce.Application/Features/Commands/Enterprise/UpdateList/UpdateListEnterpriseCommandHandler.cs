using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEnterpriseCommandHandler(IRepository<Enterprise> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateEnterpriseCommand, UpdateListEnterpriseCommand, Enterprise>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateEnterpriseCommand> GetCommandList(UpdateListEnterpriseCommand request) => request.commands;

    protected override long GetById(UpdateEnterpriseCommand command) => command.id;

    protected override void ApplyChanges(Enterprise entity, UpdateEnterpriseCommand command)
    {
        entity.UpdateInfo(command.tradeName, command.legalName, command.email, command.phone);
        entity.UpdateAddress(command.addressLine, command.city, 0, command.zipCode, command.country);
    }
}
