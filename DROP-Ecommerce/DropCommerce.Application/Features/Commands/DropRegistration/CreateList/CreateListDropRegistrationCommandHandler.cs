using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropRegistrationCommandHandler : IRequestHandler<CreateListDropRegistrationCommand, Result<List<DropRegistration>>>
{
    public Task<Result<List<DropRegistration>>> Handle(CreateListDropRegistrationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
