using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropNotificationQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropNotificationQuery, Result<DropNotification>>
{
    public async Task<Result<DropNotification>> Handle(GetByIdDropNotificationQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropNotificationQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropNotification>.Success(result.Content.First())
            : Result<DropNotification>.Failure("DropNotification não encontrado.");
    }
}
