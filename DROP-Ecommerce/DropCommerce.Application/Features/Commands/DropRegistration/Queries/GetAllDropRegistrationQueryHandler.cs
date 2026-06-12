using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropRegistrationQueryHandler : IRequestHandler<GetAllDropRegistrationQuery, Result<List<DropRegistration>>>
{
    public Task<Result<List<DropRegistration>>> Handle(GetAllDropRegistrationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
