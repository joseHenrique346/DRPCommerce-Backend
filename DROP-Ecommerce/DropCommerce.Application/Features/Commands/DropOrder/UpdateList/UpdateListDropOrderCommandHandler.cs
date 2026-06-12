using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropOrderCommandHandler : IRequestHandler<UpdateListDropOrderCommand, Result<List<DropOrder>>>
{
    public Task<Result<List<DropOrder>>> Handle(UpdateListDropOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
