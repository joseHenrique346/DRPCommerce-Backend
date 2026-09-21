using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateProductCommand, UpdateListProductCommand, Product>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateProductCommand> GetCommandList(UpdateListProductCommand request) => request.commands;

    protected override long GetById(UpdateProductCommand command) => command.id;

    protected override void ApplyChanges(Product entity, UpdateProductCommand command)
    {
        entity.UpdateDetails(command.name, command.slug, command.description, command.brand, command.imageUrls);
        entity.UpdatePricing(command.price, command.costPrice);
        entity.UpdateDimensions(command.weight, command.height, command.width, command.length);
        entity.UpdateCategory(command.categoryId, command.supplierId);
    }
}
