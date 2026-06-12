using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropTransactionCommandHandler(IMediator mediator) : IRequestHandler<CreateDropTransactionCommand, Result<DropTransaction>>
{
    public async Task<Result<DropTransaction>> Handle(CreateDropTransactionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropTransactionCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropTransaction>.Success(result.Content.First())
            : Result<DropTransaction>.Failure(result.ListMessageErrors.First());
    }
}
