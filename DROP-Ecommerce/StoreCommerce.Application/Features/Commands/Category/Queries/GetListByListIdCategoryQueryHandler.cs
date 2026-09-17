using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity.Category;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdCategoryQueryHandler(IRepository<Category> repository)
    : BaseGetListByListIdHandler<GetListByListIdCategoryQuery, Category>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdCategoryQuery request) => request.listId;
}
