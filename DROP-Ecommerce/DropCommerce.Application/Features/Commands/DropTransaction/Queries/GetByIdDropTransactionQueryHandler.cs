using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropTransactionQueryHandler(IRepository<DropTransaction> repository)
    : BaseGetByIdHandler<GetByIdDropTransactionQuery, DropTransaction>(repository)
{
    protected override long GetById(GetByIdDropTransactionQuery request) => request.id;
}
