using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllAccessRoleQueryHandler(IAccessRoleRepository repository)
    : BaseGetAllHandler<GetAllAccessRoleQuery, AccessRole>(repository)
;
