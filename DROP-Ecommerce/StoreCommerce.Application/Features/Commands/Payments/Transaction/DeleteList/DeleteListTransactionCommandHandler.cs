using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListTransactionCommandHandler(IRepository<Transaction> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListTransactionCommand, Transaction>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListTransactionCommand request) => request.ids;
}
