using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropNotificationQueryHandler : IRequestHandler<GetListByListIdDropNotificationQuery, Result<List<DropNotification>>>
{
    public Task<Result<List<DropNotification>>> Handle(GetListByListIdDropNotificationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
