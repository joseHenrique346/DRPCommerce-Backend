using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropTransactionQueryHandler(IRepository<DropTransaction> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropTransactionQuery, DropTransaction>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropTransactionQuery request) => request.listId;
}
