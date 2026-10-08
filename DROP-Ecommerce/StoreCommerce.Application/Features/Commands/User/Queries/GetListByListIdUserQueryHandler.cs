using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdUserQueryHandler(IUserRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdUserQuery, User>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdUserQuery request) => request.ids;
}
