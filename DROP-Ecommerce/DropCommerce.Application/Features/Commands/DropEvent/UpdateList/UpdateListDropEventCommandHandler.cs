using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropEventCommandHandler : IRequestHandler<UpdateListDropEventCommand, Result<List<DropEvent>>>
{
    public Task<Result<List<DropEvent>>> Handle(UpdateListDropEventCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
