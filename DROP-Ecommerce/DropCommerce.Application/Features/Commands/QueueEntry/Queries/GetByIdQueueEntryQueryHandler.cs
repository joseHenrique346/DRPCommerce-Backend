using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdQueueEntryQueryHandler(IMediator mediator) : IRequestHandler<GetByIdQueueEntryQuery, Result<QueueEntry>>
{
    public async Task<Result<QueueEntry>> Handle(GetByIdQueueEntryQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdQueueEntryQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<QueueEntry>.Success(result.Content.First())
            : Result<QueueEntry>.Failure("QueueEntry não encontrado.");
    }
}
