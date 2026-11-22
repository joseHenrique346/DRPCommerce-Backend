using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdTransactionQueryHandler(IRepository<Transaction> repository)
    : BaseGetListByListIdHandler<GetListByListIdTransactionQuery, Transaction>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdTransactionQuery request) => request.listId;
}
