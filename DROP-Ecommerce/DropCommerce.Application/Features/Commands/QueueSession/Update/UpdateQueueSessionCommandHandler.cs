using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateQueueSessionCommandHandler(IMediator mediator) : IRequestHandler<UpdateQueueSessionCommand, Result<QueueSession>>
{
    public async Task<Result<QueueSession>> Handle(UpdateQueueSessionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListQueueSessionCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<QueueSession>.Success(result.Content.First())
            : Result<QueueSession>.Failure(result.ListMessageErrors.First());
    }
}
