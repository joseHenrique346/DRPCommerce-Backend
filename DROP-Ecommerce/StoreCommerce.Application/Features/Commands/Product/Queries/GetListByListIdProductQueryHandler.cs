using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdProductQueryHandler(IRepository<Product> repository)
    : BaseGetListByListIdHandler<GetListByListIdProductQuery, Product>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdProductQuery request) => request.listId;
}
