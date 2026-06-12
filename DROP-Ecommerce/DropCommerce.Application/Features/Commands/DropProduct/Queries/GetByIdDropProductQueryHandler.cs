using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropProductQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropProductQuery, Result<DropProduct>>
{
    public async Task<Result<DropProduct>> Handle(GetByIdDropProductQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropProductQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropProduct>.Success(result.Content.First())
            : Result<DropProduct>.Failure("DropProduct não encontrado.");
    }
}
