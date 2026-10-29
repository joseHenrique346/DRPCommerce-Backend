using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReorderCategoriesCommandHandler(ICategoryRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<ReorderCategoriesCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ReorderCategoriesCommand request, CancellationToken cancellationToken)
    {
        var ids = request.Items.Select(item => item.Id).ToList();

        if (ids.Count != ids.Distinct().Count())
            return Result<bool>.Validation("Category.DuplicateIds", "A lista contém ids duplicados.");

        var tenant = tenantProvider.GetEnterpriseId();
        await using var transaction = await repository.BeginMutationAsync(tenant, cancellationToken);
        var categories = await repository.ListAsync(Category => Category.EnterpriseId == tenant && ids.Contains(Category.Id), cancellationToken);

        if (categories.Count != ids.Count)
            return Result<bool>.NotFound("Category.NotFound", "Categoria não encontrada na loja atual.");

        var byId = categories.ToDictionary(Category => Category.Id);

        foreach (var item in request.Items)
            byId[item.Id].SetDisplayOrder(item.DisplayOrder);

        await unitOfWork.CommitAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
