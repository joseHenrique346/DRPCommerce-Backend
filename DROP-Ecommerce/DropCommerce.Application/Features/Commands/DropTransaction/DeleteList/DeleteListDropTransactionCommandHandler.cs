using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropTransactionCommandHandler : IRequestHandler<DeleteListDropTransactionCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropTransactionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
