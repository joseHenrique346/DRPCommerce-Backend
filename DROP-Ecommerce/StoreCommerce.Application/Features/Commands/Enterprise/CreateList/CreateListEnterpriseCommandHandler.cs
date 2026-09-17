using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEnterpriseCommandHandler(IRepository<Enterprise> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateEnterpriseCommand, CreateListEnterpriseCommand, Enterprise>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateEnterpriseCommand> GetCommandList(CreateListEnterpriseCommand request) => request.commands;

    protected override Enterprise CreateEntity(CreateEnterpriseCommand command) =>
        Enterprise.Create(command.tradeName, command.legalName, command.email, command.phone, command.addressLine, command.city, 0, command.zipCode, command.country, command.isActive);
}
