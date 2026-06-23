using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdQueueSessionQueryHandler(IRepository<QueueSession> repository)
    : BaseGetByIdHandler<GetByIdQueueSessionQuery, QueueSession>(repository)
{
    protected override long GetById(GetByIdQueueSessionQuery request) => request.id;
}
