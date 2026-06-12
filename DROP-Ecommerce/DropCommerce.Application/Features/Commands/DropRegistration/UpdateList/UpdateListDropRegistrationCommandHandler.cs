using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropRegistrationCommandHandler : IRequestHandler<UpdateListDropRegistrationCommand, Result<List<DropRegistration>>>
{
    public Task<Result<List<DropRegistration>>> Handle(UpdateListDropRegistrationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
