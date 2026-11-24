using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropTransactionQueryHandler(IRepository<DropTransaction> repository)
    : BaseGetByIdHandler<GetByIdDropTransactionQuery, DropTransaction>(repository)
{
    protected override long GetById(GetByIdDropTransactionQuery request) => request.id;
}
