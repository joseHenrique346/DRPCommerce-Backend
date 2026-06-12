using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropOrderCommandHandler(IMediator mediator) : IRequestHandler<CreateDropOrderCommand, Result<DropOrder>>
{
    public async Task<Result<DropOrder>> Handle(CreateDropOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropOrderCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropOrder>.Success(result.Content.First())
            : Result<DropOrder>.Failure(result.ListMessageErrors.First());
    }
}
