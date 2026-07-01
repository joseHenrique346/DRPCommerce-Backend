using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropRegistrationQueryHandler(IRepository<DropRegistration> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropRegistrationQuery, DropRegistration>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropRegistrationQuery request) => request.listId;
}
