using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropTransactionCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropTransactionCommand, Result<DropTransaction>>
{
    public async Task<Result<DropTransaction>> Handle(UpdateDropTransactionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropTransactionCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropTransaction>.Success(result.Content.First())
            : Result<DropTransaction>.Failure(result.ListMessageErrors.First());
    }
}
