using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropOrderItemCommandHandler : IRequestHandler<CreateListDropOrderItemCommand, Result<List<DropOrderItem>>>
{
    public Task<Result<List<DropOrderItem>>> Handle(CreateListDropOrderItemCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
