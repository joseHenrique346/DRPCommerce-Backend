using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropTransactionCommandHandler : IRequestHandler<UpdateListDropTransactionCommand, Result<List<DropTransaction>>>
{
    public Task<Result<List<DropTransaction>>> Handle(UpdateListDropTransactionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
