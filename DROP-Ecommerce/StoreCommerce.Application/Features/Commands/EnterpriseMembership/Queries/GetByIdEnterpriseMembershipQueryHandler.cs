using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEnterpriseMembershipQueryHandler(IEnterpriseMembershipRepository repository)
    : BaseGetByIdHandler<GetByIdEnterpriseMembershipQuery, EnterpriseMembership>(repository)
{
    protected override long GetById(GetByIdEnterpriseMembershipQuery request) => request.id;
}
