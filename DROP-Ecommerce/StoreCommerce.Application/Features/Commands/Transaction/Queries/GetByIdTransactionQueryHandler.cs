using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdTransactionQueryHandler(IRepository<Transaction> repository)
    : BaseGetByIdHandler<GetByIdTransactionQuery, Transaction>(repository)
{
    protected override long GetById(GetByIdTransactionQuery request) => request.id;
}
