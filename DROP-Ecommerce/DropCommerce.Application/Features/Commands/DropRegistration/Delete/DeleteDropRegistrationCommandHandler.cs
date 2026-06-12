using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropRegistrationCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropRegistrationCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropRegistrationCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropRegistrationCommand([request.id]), cancellationToken);
    }
}
