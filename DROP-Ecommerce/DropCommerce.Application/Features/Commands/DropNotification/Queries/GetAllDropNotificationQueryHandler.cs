using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropNotificationQueryHandler : IRequestHandler<GetAllDropNotificationQuery, Result<List<DropNotification>>>
{
    public Task<Result<List<DropNotification>>> Handle(GetAllDropNotificationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
