using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropRegistrationQueryHandler(IRepository<DropRegistration> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropRegistrationQuery, DropRegistration>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropRegistrationQuery request) => request.listId;
}
