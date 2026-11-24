using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdDocumentQueryHandler(IRepository<Document> repository)
    : BaseGetListByListIdHandler<GetListByListIdDocumentQuery, Document>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDocumentQuery request) => request.listId;
}
