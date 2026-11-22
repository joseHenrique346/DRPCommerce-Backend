using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllDocumentQueryHandler(IRepository<Document> repository)
    : BaseGetAllHandler<GetAllDocumentQuery, Document>(repository);
