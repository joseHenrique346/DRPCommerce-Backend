using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListServiceCommandHandler(IRepository<Service> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateServiceCommand, CreateListServiceCommand, Service>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateServiceCommand> GetCommandList(CreateListServiceCommand request) => request.commands;

    protected override Service CreateEntity(CreateServiceCommand command) =>
        Service.Create(command.enterpriseId, command.categoryId, command.name, command.description, command.price, command.durationMinutes, command.isActive);
}
