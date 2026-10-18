using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateEmployeePositionCommandHandler(IMediator mediator) : IRequestHandler<UpdateEmployeePositionCommand, Result<EmployeePosition>>
{
    public async Task<Result<EmployeePosition>> Handle(UpdateEmployeePositionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListEmployeePositionCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<EmployeePosition>.Success(result.Content![0])
            : Result<EmployeePosition>.Failure(result.Errors);
    }
}
