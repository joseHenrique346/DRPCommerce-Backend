using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropProductCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropProductCommand, Result<DropProduct>>
{
    public async Task<Result<DropProduct>> Handle(UpdateDropProductCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropProductCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropProduct>.Success(result.Content.First())
            : Result<DropProduct>.Failure(result.ListMessageErrors.First());
    }
}
