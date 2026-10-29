using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateListCategoryCommandHandler(ICategoryRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<UpdateListCategoryCommand, Result<List<Category>>>
{
    public async Task<Result<List<Category>>> Handle(UpdateListCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.commands.Select(command => command.id).Distinct().Count() != request.commands.Count)
            return Result<List<Category>>.Validation("Category.DuplicateIds", "A lista contém ids duplicados.");

        var tenant = tenantProvider.GetEnterpriseId();
        await using var transaction = await repository.BeginMutationAsync(tenant, cancellationToken);
        var current = await repository.ListAsync(Category => Category.EnterpriseId == tenant, cancellationToken);
        var byId = current.ToDictionary(Category => Category.Id);
        var updates = new Dictionary<long, Category>();

        foreach (var command in request.commands)
        {
            if (!byId.ContainsKey(command.id))
                return Result<List<Category>>.NotFound("Category.NotFound", "Categoria não encontrada na loja atual.");

            updates[command.id] = Category.Create(
                tenant, command.parentCategoryId, command.name, command.slug,
                command.description, command.imageUrl, command.displayOrder, command.isActive);
        }

        var error = CategoryRules.ValidateChanges(current, updates, Array.Empty<Category>());

        if (error is not null)
            return Result<List<Category>>.Failure(error);

        var changed = new List<Category>();

        foreach (var command in request.commands)
        {
            var category = byId[command.id];
            var update = updates[command.id];
            category.UpdateDetails(update.ParentCategoryId, update.Name, update.Slug, update.Description,
                update.ImageUrl, update.DisplayOrder, update.IsActive);
            changed.Add(category);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result<List<Category>>.Success(changed);
    }
}
