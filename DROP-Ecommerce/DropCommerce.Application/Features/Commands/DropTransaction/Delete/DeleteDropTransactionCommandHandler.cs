using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropTransactionCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropTransactionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropTransactionCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropTransactionCommand([request.id]), cancellationToken);
    }
}
