using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropOrderItemCommandHandler : IRequestHandler<DeleteListDropOrderItemCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropOrderItemCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
