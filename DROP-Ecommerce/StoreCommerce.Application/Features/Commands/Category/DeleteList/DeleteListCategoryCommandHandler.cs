using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListCategoryCommandHandler(IRepository<Category> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListCategoryCommand, Category>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListCategoryCommand request) => request.ids;
}
