using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropEventCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropEventCommand, Result<DropEvent>>
{
    public async Task<Result<DropEvent>> Handle(UpdateDropEventCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropEventCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropEvent>.Success(result.Content.First())
            : Result<DropEvent>.Failure(result.ListMessageErrors.First());
    }
}
