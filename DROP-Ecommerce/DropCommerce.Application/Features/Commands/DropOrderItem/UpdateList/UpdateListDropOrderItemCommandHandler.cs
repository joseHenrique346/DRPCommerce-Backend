using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropOrderItemCommandHandler : IRequestHandler<UpdateListDropOrderItemCommand, Result<List<DropOrderItem>>>
{
    public Task<Result<List<DropOrderItem>>> Handle(UpdateListDropOrderItemCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
