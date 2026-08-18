using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropProductCommandHandler(IRepository<DropProduct> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropProductCommand, UpdateListDropProductCommand, DropProduct>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropProductCommand> GetCommandList(UpdateListDropProductCommand request) => request.commands;

    protected override long GetById(UpdateDropProductCommand command) => command.id;

    protected override void ApplyChanges(DropProduct entity, UpdateDropProductCommand command)
    {
        entity.Update(command.dropEventId, command.productId, command.sku, command.unitsAllocated, command.unitsSold, command.maxPerCustomer, command.price, command.isActive);
    }
}
