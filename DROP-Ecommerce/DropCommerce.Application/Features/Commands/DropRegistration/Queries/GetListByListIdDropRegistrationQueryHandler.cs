using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropRegistrationQueryHandler : IRequestHandler<GetListByListIdDropRegistrationQuery, Result<List<DropRegistration>>>
{
    public Task<Result<List<DropRegistration>>> Handle(GetListByListIdDropRegistrationQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
