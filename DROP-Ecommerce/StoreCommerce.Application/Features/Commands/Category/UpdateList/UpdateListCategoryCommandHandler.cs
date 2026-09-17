using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity.Category;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCategoryCommandHandler(IRepository<Category> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateCategoryCommand, UpdateListCategoryCommand, Category>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateCategoryCommand> GetCommandList(UpdateListCategoryCommand request) => request.commands;

    protected override long GetById(UpdateCategoryCommand command) => command.id;

    protected override void ApplyChanges(Category entity, UpdateCategoryCommand command)
    {
        entity.UpdateDetails(command.parentCategoryId, command.name, command.slug, command.dscription, command.imageUrl, command.displayOrder);
    }
}
