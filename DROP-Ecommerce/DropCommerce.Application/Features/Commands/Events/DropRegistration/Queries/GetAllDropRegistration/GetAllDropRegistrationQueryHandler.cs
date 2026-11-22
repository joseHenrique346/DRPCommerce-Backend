using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropRegistrationQueryHandler(IRepository<DropRegistration> repository)
    : BaseGetAllHandler<GetAllDropRegistrationQuery, DropRegistration>(repository);
