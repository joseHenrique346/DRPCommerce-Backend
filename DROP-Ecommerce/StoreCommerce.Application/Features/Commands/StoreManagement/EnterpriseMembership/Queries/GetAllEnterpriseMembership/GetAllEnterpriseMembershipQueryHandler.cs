using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllEnterpriseMembershipQueryHandler(IEnterpriseMembershipRepository repository)
    : BaseGetAllHandler<GetAllEnterpriseMembershipQuery, EnterpriseMembership>(repository)
;
