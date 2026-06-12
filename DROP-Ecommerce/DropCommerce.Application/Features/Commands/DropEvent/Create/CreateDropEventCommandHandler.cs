using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropEventCommandHandler(IMediator mediator) : IRequestHandler<CreateDropEventCommand, Result<DropEvent>>
{
    public async Task<Result<DropEvent>> Handle(CreateDropEventCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropEventCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropEvent>.Success(result.Content.First())
            : Result<DropEvent>.Failure(result.ListMessageErrors.First());
    }
}
