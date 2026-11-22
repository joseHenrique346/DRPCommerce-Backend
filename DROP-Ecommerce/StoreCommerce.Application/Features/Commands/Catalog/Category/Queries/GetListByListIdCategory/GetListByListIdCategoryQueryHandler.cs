using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdCategoryQueryHandler(IRepository<Category> repository)
    : BaseGetListByListIdHandler<GetListByListIdCategoryQuery, Category>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdCategoryQuery request) => request.listId;
}
