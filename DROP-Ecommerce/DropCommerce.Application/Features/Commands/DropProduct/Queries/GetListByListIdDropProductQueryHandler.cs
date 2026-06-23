using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropProductQueryHandler(IRepository<DropProduct> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropProductQuery, DropProduct>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropProductQuery request) => request.listId;
}
