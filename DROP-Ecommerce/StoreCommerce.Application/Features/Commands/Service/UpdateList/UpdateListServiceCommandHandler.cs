using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListServiceCommandHandler(IRepository<Service> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateServiceCommand, UpdateListServiceCommand, Service>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateServiceCommand> GetCommandList(UpdateListServiceCommand request) => request.commands;

    protected override long GetById(UpdateServiceCommand command) => command.id;

    protected override void ApplyChanges(Service entity, UpdateServiceCommand command)
    {
        entity.UpdateDetails(command.categoryId, command.name, command.description, command.price, command.durationMinutes);
    }
}
