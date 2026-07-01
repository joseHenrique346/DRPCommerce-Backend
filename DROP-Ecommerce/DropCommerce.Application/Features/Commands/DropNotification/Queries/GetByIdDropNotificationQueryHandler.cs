using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropNotificationQueryHandler(IRepository<DropNotification> repository)
    : BaseGetByIdHandler<GetByIdDropNotificationQuery, DropNotification>(repository)
{
    protected override long GetById(GetByIdDropNotificationQuery request) => request.id;
}
