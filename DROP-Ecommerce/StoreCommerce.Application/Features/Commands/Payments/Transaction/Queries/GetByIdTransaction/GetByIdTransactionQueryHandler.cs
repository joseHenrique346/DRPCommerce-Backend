using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdTransactionQueryHandler(IRepository<Transaction> repository)
    : BaseGetByIdHandler<GetByIdTransactionQuery, Transaction>(repository)
{
    protected override long GetById(GetByIdTransactionQuery request) => request.id;
}
