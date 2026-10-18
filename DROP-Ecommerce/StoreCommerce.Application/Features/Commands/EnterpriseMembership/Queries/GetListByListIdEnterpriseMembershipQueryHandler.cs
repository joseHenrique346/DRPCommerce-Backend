using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdEnterpriseMembershipQueryHandler(IEnterpriseMembershipRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdEnterpriseMembershipQuery, EnterpriseMembership>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdEnterpriseMembershipQuery request) => request.ids;
}
