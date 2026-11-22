using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdInvoiceQueryHandler(IRepository<Invoice> repository)
    : BaseGetListByListIdHandler<GetListByListIdInvoiceQuery, Invoice>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdInvoiceQuery request) => request.listId;
}
