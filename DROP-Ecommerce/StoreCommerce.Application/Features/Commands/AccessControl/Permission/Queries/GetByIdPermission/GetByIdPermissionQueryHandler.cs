using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdPermissionQueryHandler(IPermissionRepository repository)
    : BaseGetByIdHandler<GetByIdPermissionQuery, Permission>(repository)
{
    protected override long GetById(GetByIdPermissionQuery request) => request.id;
}
