using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdRoleQueryHandler(IRepository<Role> repository)
    : BaseGetByIdHandler<GetByIdRoleQuery, Role>(repository)
{
    protected override long GetById(GetByIdRoleQuery request) => request.id;
}
