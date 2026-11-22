using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdEnterpriseMembershipQueryHandler(IEnterpriseMembershipRepository repository)
    : BaseGetListByListIdHandler<GetListByListIdEnterpriseMembershipQuery, EnterpriseMembership>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdEnterpriseMembershipQuery request) => request.ids;
}
