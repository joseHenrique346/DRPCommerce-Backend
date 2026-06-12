using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropProductCommandHandler : IRequestHandler<UpdateListDropProductCommand, Result<List<DropProduct>>>
{
    public Task<Result<List<DropProduct>>> Handle(UpdateListDropProductCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
