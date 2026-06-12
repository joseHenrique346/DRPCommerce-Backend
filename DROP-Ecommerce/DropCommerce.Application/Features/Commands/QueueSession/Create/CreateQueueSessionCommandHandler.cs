using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateQueueSessionCommandHandler(IMediator mediator) : IRequestHandler<CreateQueueSessionCommand, Result<QueueSession>>
{
    public async Task<Result<QueueSession>> Handle(CreateQueueSessionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListQueueSessionCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<QueueSession>.Success(result.Content.First())
            : Result<QueueSession>.Failure(result.ListMessageErrors.First());
    }
}
