using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdFraudSignalQueryHandler(IMediator mediator) : IRequestHandler<GetByIdFraudSignalQuery, Result<FraudSignal>>
{
    public async Task<Result<FraudSignal>> Handle(GetByIdFraudSignalQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdFraudSignalQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<FraudSignal>.Success(result.Content.First())
            : Result<FraudSignal>.Failure("FraudSignal não encontrado.");
    }
}
