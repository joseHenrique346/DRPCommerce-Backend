using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropTransactionCommandHandler : IRequestHandler<CreateListDropTransactionCommand, Result<List<DropTransaction>>>
{
    public Task<Result<List<DropTransaction>>> Handle(CreateListDropTransactionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
