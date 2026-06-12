using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropRegistrationQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropRegistrationQuery, Result<DropRegistration>>
{
    public async Task<Result<DropRegistration>> Handle(GetByIdDropRegistrationQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropRegistrationQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropRegistration>.Success(result.Content.First())
            : Result<DropRegistration>.Failure("DropRegistration não encontrado.");
    }
}
