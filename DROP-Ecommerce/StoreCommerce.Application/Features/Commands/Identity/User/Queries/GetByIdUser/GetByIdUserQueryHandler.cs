using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdUserQueryHandler(IUserRepository repository)
    : BaseGetByIdHandler<GetByIdUserQuery, User>(repository)
{
    protected override long GetById(GetByIdUserQuery request) => request.id;
}
