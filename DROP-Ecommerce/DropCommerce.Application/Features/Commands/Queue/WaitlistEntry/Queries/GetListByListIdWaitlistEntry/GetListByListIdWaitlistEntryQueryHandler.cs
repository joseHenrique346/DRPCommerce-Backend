using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdWaitlistEntryQueryHandler(IRepository<WaitlistEntry> repository)
    : BaseGetListByListIdHandler<GetListByListIdWaitlistEntryQuery, WaitlistEntry>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdWaitlistEntryQuery request) => request.listId;
}
