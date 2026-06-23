using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropRegistrationQueryHandler(IRepository<DropRegistration> repository)
    : BaseGetByIdHandler<GetByIdDropRegistrationQuery, DropRegistration>(repository)
{
    protected override long GetById(GetByIdDropRegistrationQuery request) => request.id;
}
