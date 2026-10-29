using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateListCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<CreateListCategoryCommand, Result<List<Category>>>
{
    public async Task<Result<List<Category>>> Handle(CreateListCategoryCommand request, CancellationToken cancellationToken)
    {
        var tenant = tenantProvider.GetEnterpriseId();
        await using var transaction = await repository.BeginMutationAsync(tenant, cancellationToken);
        var current = await repository.ListAsync(Category => Category.EnterpriseId == tenant, cancellationToken);
        var additions = request.commands.Select(command => Category.Create(
            tenant, command.parentCategoryId, command.name, command.slug,
            command.description, command.imageUrl, command.displayOrder, command.isActive)).ToList();
        var error = CategoryRules.ValidateChanges(current, new Dictionary<long, Category>(), additions);

        if (error is not null)
            return Result<List<Category>>.Failure(error);

        await repository.AddRangeAsync(additions, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<List<Category>>.Success(additions);
    }
}
