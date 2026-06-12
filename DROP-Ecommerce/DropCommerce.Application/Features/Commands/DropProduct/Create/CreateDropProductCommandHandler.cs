using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropProductCommandHandler(IMediator mediator) : IRequestHandler<CreateDropProductCommand, Result<DropProduct>>
{
    public async Task<Result<DropProduct>> Handle(CreateDropProductCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropProductCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropProduct>.Success(result.Content.First())
            : Result<DropProduct>.Failure(result.ListMessageErrors.First());
    }
}
