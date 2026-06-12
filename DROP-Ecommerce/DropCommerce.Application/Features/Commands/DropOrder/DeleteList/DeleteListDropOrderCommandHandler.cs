using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropOrderCommandHandler : IRequestHandler<DeleteListDropOrderCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
