using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropRegistrationQueryHandler(IRepository<DropRegistration> repository)
    : BaseGetByIdHandler<GetByIdDropRegistrationQuery, DropRegistration>(repository)
{
    protected override long GetById(GetByIdDropRegistrationQuery request) => request.id;
}
