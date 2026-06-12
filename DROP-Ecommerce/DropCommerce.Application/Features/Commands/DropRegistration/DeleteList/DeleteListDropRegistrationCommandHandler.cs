using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropRegistrationCommandHandler : IRequestHandler<DeleteListDropRegistrationCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropRegistrationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
