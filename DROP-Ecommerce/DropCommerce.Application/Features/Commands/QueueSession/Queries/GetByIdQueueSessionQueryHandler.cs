using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdQueueSessionQueryHandler(IMediator mediator) : IRequestHandler<GetByIdQueueSessionQuery, Result<QueueSession>>
{
    public async Task<Result<QueueSession>> Handle(GetByIdQueueSessionQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdQueueSessionQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<QueueSession>.Success(result.Content.First())
            : Result<QueueSession>.Failure("QueueSession não encontrado.");
    }
}
