using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropOrderCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropOrderCommand, Result<DropOrder>>
{
    public async Task<Result<DropOrder>> Handle(UpdateDropOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropOrderCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropOrder>.Success(result.Content.First())
            : Result<DropOrder>.Failure(result.ListMessageErrors.First());
    }
}
