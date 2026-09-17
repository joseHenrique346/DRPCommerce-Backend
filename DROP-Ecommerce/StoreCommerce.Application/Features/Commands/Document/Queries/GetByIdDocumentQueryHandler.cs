using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdDocumentQueryHandler(IRepository<Document> repository)
    : BaseGetByIdHandler<GetByIdDocumentQuery, Document>(repository)
{
    protected override long GetById(GetByIdDocumentQuery request) => request.id;
}
