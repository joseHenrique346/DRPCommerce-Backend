using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity.Category;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCategoryCommandHandler(IRepository<Category> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateCategoryCommand, CreateListCategoryCommand, Category>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateCategoryCommand> GetCommandList(CreateListCategoryCommand request) => request.commands;

    protected override Category CreateEntity(CreateCategoryCommand command) =>
        Category.Create(command.enterpriseId, command.parentCategoryId, command.name, command.slug, command.dscription, command.imageUrl, command.displayOrder, command.isActive);
}
