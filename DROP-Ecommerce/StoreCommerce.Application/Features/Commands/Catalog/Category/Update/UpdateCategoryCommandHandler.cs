using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository repository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : IRequestHandler<UpdateCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var commands = new List<UpdateCategoryCommand> { request };

        if (commands.Select(command => command.id).Distinct().Count() != commands.Count)
            return Result<Category>.Validation("Category.DuplicateIds", "A lista contém ids duplicados.");

        var tenant = tenantProvider.GetEnterpriseId();
        await using var transaction = await repository.BeginMutationAsync(tenant, cancellationToken);
        var current = await repository.ListAsync(Category => Category.EnterpriseId == tenant, cancellationToken);
        var byId = current.ToDictionary(Category => Category.Id);
        var updates = new Dictionary<long, Category>();

        foreach (var command in commands)
        {
            if (!byId.ContainsKey(command.id))
                return Result<Category>.NotFound("Category.NotFound", "Categoria não encontrada na loja atual.");

            updates[command.id] = Category.Create(
                tenant, command.parentCategoryId, command.name, command.slug,
                command.description, command.imageUrl, command.displayOrder, command.isActive);
        }

        var error = CategoryRules.ValidateChanges(current, updates, Array.Empty<Category>());

        if (error is not null)
            return Result<Category>.Failure(error);

        var changed = new List<Category>();

        foreach (var command in commands)
        {
            var category = byId[command.id];
            var update = updates[command.id];
            category.UpdateDetails(update.ParentCategoryId, update.Name, update.Slug, update.Description,
                update.ImageUrl, update.DisplayOrder, update.IsActive);
            changed.Add(category);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<Category>.Success(changed[0]);
    }
}
