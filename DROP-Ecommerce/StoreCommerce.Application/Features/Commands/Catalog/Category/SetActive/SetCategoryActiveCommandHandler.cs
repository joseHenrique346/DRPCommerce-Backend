using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Persistence;
using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetCategoryActiveCommandHandler(ICategoryRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<SetCategoryActiveCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(SetCategoryActiveCommand request, CancellationToken cancellationToken)
    {
        var tenant = tenantProvider.GetEnterpriseId();
        await using var transaction = await repository.BeginMutationAsync(tenant, cancellationToken);
        var category = await repository.FindAsync(Category => Category.Id == request.Id && Category.EnterpriseId == tenant, cancellationToken);

        if (category is null)
            return Result<Category>.NotFound("Category.NotFound", "Categoria não encontrada na loja atual.");

        category.SetActive(request.IsActive);
        await unitOfWork.CommitAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<Category>.Success(category);
    }
}
