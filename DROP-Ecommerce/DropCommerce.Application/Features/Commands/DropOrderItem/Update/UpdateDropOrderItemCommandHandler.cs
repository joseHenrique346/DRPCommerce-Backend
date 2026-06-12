using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropOrderItemCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropOrderItemCommand, Result<DropOrderItem>>
{
    public async Task<Result<DropOrderItem>> Handle(UpdateDropOrderItemCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropOrderItemCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropOrderItem>.Success(result.Content.First())
            : Result<DropOrderItem>.Failure(result.ListMessageErrors.First());
    }
}
