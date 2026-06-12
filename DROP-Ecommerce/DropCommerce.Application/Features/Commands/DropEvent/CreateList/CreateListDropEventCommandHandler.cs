using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropEventCommandHandler : IRequestHandler<CreateListDropEventCommand, Result<List<DropEvent>>>
{
    public Task<Result<List<DropEvent>>> Handle(CreateListDropEventCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
