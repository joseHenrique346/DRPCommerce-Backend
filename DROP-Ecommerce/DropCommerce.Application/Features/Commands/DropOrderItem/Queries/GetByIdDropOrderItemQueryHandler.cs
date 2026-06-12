using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropOrderItemQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropOrderItemQuery, Result<DropOrderItem>>
{
    public async Task<Result<DropOrderItem>> Handle(GetByIdDropOrderItemQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropOrderItemQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropOrderItem>.Success(result.Content.First())
            : Result<DropOrderItem>.Failure("DropOrderItem não encontrado.");
    }
}
