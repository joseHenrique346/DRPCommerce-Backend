using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateProductCommand, CreateListProductCommand, Product>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateProductCommand> GetCommandList(CreateListProductCommand request) => request.commands;

    protected override Product CreateEntity(CreateProductCommand command) =>
        Product.Create(tenantProvider.GetEnterpriseId(), command.categoryId, command.supplierId, command.name, command.slug, command.description, command.sku, command.barCode, command.price, command.costPrice, command.weight, command.height, command.width, command.length, command.brand, command.imageUrls, command.isActive, command.isDigital);
}
