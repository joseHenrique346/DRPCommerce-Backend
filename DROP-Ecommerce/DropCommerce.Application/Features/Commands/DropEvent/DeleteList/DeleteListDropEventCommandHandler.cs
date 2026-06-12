using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropEventCommandHandler : IRequestHandler<DeleteListDropEventCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropEventCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
