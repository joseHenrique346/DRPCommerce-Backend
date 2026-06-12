using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropOrderQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropOrderQuery, Result<DropOrder>>
{
    public async Task<Result<DropOrder>> Handle(GetByIdDropOrderQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropOrderQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropOrder>.Success(result.Content.First())
            : Result<DropOrder>.Failure("DropOrder não encontrado.");
    }
}
