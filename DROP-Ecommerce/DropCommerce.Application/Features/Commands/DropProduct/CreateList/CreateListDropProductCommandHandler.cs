using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropProductCommandHandler(IRepository<DropProduct> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropProductCommand, CreateListDropProductCommand, DropProduct>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropProductCommand> GetCommandList(CreateListDropProductCommand request) => request.commands;

    protected override DropProduct CreateEntity(CreateDropProductCommand command) =>
        DropProduct.Create(command.dropEventId, command.productId, command.sku, command.unitsAllocated, command.unitsSold, command.maxPerCustomer, command.price, command.isActive);
}
