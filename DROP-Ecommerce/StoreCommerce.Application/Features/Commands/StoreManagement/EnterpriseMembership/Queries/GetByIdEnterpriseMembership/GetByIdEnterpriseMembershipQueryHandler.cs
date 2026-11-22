using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEnterpriseMembershipQueryHandler(IEnterpriseMembershipRepository repository)
    : BaseGetByIdHandler<GetByIdEnterpriseMembershipQuery, EnterpriseMembership>(repository)
{
    protected override long GetById(GetByIdEnterpriseMembershipQuery request) => request.id;
}
