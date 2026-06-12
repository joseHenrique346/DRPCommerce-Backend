using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropTransactionQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropTransactionQuery, Result<DropTransaction>>
{
    public async Task<Result<DropTransaction>> Handle(GetByIdDropTransactionQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropTransactionQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropTransaction>.Success(result.Content.First())
            : Result<DropTransaction>.Failure("DropTransaction não encontrado.");
    }
}
