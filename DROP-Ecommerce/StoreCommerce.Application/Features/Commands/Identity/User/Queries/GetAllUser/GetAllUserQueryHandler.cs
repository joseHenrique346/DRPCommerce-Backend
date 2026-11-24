using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllUserQueryHandler(IUserRepository repository)
    : BaseGetAllHandler<GetAllUserQuery, User>(repository)
;
