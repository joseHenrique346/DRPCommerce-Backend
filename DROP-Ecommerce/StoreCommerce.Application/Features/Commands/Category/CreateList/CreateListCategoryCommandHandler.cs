using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCategoryCommandHandler(IRepository<Category> repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateCategoryCommand, CreateListCategoryCommand, Category>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateCategoryCommand> GetCommandList(CreateListCategoryCommand request) => request.commands;

    protected override Category CreateEntity(CreateCategoryCommand command) =>
        Category.Create(tenantProvider.GetEnterpriseId(), command.parentCategoryId, command.name, command.slug, command.description, command.imageUrl, command.displayOrder, command.isActive);
}
