using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdAccessRoleQueryHandler(IAccessRoleRepository repository)
    : BaseGetByIdHandler<GetByIdAccessRoleQuery, AccessRole>(repository)
{
    protected override long GetById(GetByIdAccessRoleQuery request) => request.id;
}
