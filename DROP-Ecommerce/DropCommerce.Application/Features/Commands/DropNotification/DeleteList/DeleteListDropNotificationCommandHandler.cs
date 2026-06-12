using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropNotificationCommandHandler : IRequestHandler<DeleteListDropNotificationCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropNotificationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
