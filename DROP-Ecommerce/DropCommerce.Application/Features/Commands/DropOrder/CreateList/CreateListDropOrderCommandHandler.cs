using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropOrderCommandHandler : IRequestHandler<CreateListDropOrderCommand, Result<List<DropOrder>>>
{
    public Task<Result<List<DropOrder>>> Handle(CreateListDropOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
