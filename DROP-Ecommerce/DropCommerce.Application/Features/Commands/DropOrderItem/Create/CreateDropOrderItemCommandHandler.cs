using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropOrderItemCommandHandler(IMediator mediator) : IRequestHandler<CreateDropOrderItemCommand, Result<DropOrderItem>>
{
    public async Task<Result<DropOrderItem>> Handle(CreateDropOrderItemCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropOrderItemCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropOrderItem>.Success(result.Content.First())
            : Result<DropOrderItem>.Failure(result.ListMessageErrors.First());
    }
}
