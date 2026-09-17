using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdDocumentQueryHandler(IRepository<Document> repository)
    : BaseGetListByListIdHandler<GetListByListIdDocumentQuery, Document>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDocumentQuery request) => request.listId;
}
