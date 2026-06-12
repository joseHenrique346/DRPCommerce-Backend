using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropProductCommandHandler : IRequestHandler<CreateListDropProductCommand, Result<List<DropProduct>>>
{
    public Task<Result<List<DropProduct>>> Handle(CreateListDropProductCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
