using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropReservationCommandHandler : IRequestHandler<DeleteListDropReservationCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropReservationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
