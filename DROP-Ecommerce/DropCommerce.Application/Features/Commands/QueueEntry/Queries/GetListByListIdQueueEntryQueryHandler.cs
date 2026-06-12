using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdQueueEntryQueryHandler : IRequestHandler<GetListByListIdQueueEntryQuery, Result<List<QueueEntry>>>
{
    public Task<Result<List<QueueEntry>>> Handle(GetListByListIdQueueEntryQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
