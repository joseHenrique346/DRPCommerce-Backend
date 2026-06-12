using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropEventQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropEventQuery, Result<DropEvent>>
{
    public async Task<Result<DropEvent>> Handle(GetByIdDropEventQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropEventQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropEvent>.Success(result.Content.First())
            : Result<DropEvent>.Failure("DropEvent não encontrado.");
    }
}
