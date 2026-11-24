using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllPermissionQueryHandler(IPermissionRepository repository)
    : BaseGetAllHandler<GetAllPermissionQuery, Permission>(repository);
