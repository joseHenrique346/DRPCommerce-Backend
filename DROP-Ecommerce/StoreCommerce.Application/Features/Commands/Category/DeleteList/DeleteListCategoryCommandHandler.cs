using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteListCategoryCommandHandler(
    ICategoryRepository repository,
    IRepository<Product> products,
    IRepository<Service> services,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : IRequestHandler<DeleteListCategoryCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteListCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ids.Count != request.ids.Distinct().Count())
            return Result<bool>.Validation("Category.DuplicateIds", "A lista contém ids duplicados.");

        var tenant = tenantProvider.GetEnterpriseId();
        await using var transaction = await repository.BeginMutationAsync(tenant, cancellationToken);
        var categories = await repository.ListAsync(Category => Category.EnterpriseId == tenant, cancellationToken);
        var selected = categories.Where(Category => request.ids.Contains(Category.Id)).ToList();

        if (selected.Count != request.ids.Count)
            return Result<bool>.NotFound("Category.NotFound", "Categoria não encontrada na loja atual.");

        if (categories.Any(Category => Category.ParentCategoryId.HasValue && request.ids.Contains(Category.ParentCategoryId.Value) && !request.ids.Contains(Category.Id)))
            return Result<bool>.Conflict("Category.HasChildren", "A categoria possui subcategorias não excluídas.");

        if (await services.ExistsAsync(Service => Service.EnterpriseId == tenant && request.ids.Contains(Service.CategoryId), cancellationToken))
            return Result<bool>.Conflict("Category.HasServices", "Mova ou exclua os serviços da categoria antes de excluí-la.");

        var linkedProducts = await products.ListAsync(
            Product => Product.EnterpriseId == tenant && request.ids.Contains(Product.CategoryId), cancellationToken);
        Category? destination = null;

        if (request.moveProductsToCategoryId is long targetId)
        {
            destination = CategoryRules.VisibleCategories(categories).SingleOrDefault(Category => Category.Id == targetId);

            if (destination is null || request.ids.Contains(targetId))
                return Result<bool>.Validation("Category.InvalidDestination", "A categoria de destino deve estar disponível, fora da exclusão e pertencer à mesma loja.");
        }

        if (linkedProducts.Count > 0 && destination is null)
            return Result<bool>.Conflict("Category.HasProducts", "A categoria possui produtos não excluídos. Informe uma categoria de destino.");

        foreach (var product in linkedProducts)
            product.UpdateCategory(destination!.Id, product.SupplierId);

        foreach (var category in selected)
            category.SoftDelete();

        await unitOfWork.CommitAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
