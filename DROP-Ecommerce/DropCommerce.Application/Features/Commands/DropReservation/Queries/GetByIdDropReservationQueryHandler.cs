using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropReservationQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropReservationQuery, Result<DropReservation>>
{
    public async Task<Result<DropReservation>> Handle(GetByIdDropReservationQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropReservationQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropReservation>.Success(result.Content.First())
            : Result<DropReservation>.Failure("DropReservation não encontrado.");
    }
}
