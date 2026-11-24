using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropNotificationQueryHandler(IRepository<DropNotification> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropNotificationQuery, DropNotification>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropNotificationQuery request) => request.listId;
}
