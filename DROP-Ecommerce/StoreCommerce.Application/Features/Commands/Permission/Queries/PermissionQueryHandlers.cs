using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllPermissionQueryHandler(IPermissionRepository repository)
    : BaseGetAllHandler<GetAllPermissionQuery, Permission>(repository);

public class GetByIdPermissionQueryHandler(IPermissionRepository repository)
    : BaseGetByIdHandler<GetByIdPermissionQuery, Permission>(repository)
{
    protected override long GetById(GetByIdPermissionQuery request) => request.id;
}
