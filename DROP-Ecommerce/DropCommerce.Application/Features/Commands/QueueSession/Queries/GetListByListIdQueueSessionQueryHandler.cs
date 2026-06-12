using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdQueueSessionQueryHandler : IRequestHandler<GetListByListIdQueueSessionQuery, Result<List<QueueSession>>>
{
    public Task<Result<List<QueueSession>>> Handle(GetListByListIdQueueSessionQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
