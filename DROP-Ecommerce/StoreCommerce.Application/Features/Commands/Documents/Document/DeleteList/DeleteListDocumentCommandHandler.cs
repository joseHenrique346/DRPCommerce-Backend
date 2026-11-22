using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListDocumentCommandHandler(IRepository<Document> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDocumentCommand, Document>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDocumentCommand request) => request.ids;
}
