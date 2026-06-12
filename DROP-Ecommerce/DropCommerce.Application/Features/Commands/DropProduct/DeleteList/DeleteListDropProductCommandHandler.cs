using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropProductCommandHandler : IRequestHandler<DeleteListDropProductCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropProductCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
