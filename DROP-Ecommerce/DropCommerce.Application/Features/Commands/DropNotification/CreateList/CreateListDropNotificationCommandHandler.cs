using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropNotificationCommandHandler : IRequestHandler<CreateListDropNotificationCommand, Result<List<DropNotification>>>
{
    public Task<Result<List<DropNotification>>> Handle(CreateListDropNotificationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
