using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdSupplierQueryHandler(IRepository<Supplier> repository)
    : BaseGetListByListIdHandler<GetListByListIdSupplierQuery, Supplier>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdSupplierQuery request) => request.listId;
}
