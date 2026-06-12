using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropNotificationCommandHandler : IRequestHandler<UpdateListDropNotificationCommand, Result<List<DropNotification>>>
{
    public Task<Result<List<DropNotification>>> Handle(UpdateListDropNotificationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
