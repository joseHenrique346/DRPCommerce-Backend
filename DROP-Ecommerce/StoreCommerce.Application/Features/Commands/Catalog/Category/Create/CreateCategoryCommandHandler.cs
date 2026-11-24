using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : IRequestHandler<CreateCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var commands = new List<CreateCategoryCommand> { request };

        var tenant = tenantProvider.GetEnterpriseId();
        await using var transaction = await repository.BeginMutationAsync(tenant, cancellationToken);
        var current = await repository.ListAsync(Category => Category.EnterpriseId == tenant, cancellationToken);
        var additions = commands.Select(command => Category.Create(
            tenant, command.parentCategoryId, command.name, command.slug,
            command.description, command.imageUrl, command.displayOrder, command.isActive)).ToList();
        var error = CategoryRules.ValidateChanges(current, new Dictionary<long, Category>(), additions);

        if (error is not null)
            return Result<Category>.Failure(error);

        await repository.AddRangeAsync(additions, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<Category>.Success(additions[0]);
    }
}
