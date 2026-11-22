using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdQueueSessionQueryHandler(IRepository<QueueSession> repository)
    : BaseGetByIdHandler<GetByIdQueueSessionQuery, QueueSession>(repository)
{
    protected override long GetById(GetByIdQueueSessionQuery request) => request.id;
}
