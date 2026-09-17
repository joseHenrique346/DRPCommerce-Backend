using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllDocumentQueryHandler(IRepository<Document> repository)
    : BaseGetAllHandler<GetAllDocumentQuery, Document>(repository);
