using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdWaitlistEntryQueryHandler(IMediator mediator) : IRequestHandler<GetByIdWaitlistEntryQuery, Result<WaitlistEntry>>
{
    public async Task<Result<WaitlistEntry>> Handle(GetByIdWaitlistEntryQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdWaitlistEntryQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<WaitlistEntry>.Success(result.Content.First())
            : Result<WaitlistEntry>.Failure("WaitlistEntry não encontrado.");
    }
}
