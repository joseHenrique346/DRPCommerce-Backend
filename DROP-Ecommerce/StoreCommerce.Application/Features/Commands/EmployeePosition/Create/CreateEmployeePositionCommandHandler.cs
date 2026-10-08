using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class CreateEmployeePositionCommandHandler(IMediator mediator) : IRequestHandler<CreateEmployeePositionCommand, Result<EmployeePosition>>
{
    public async Task<Result<EmployeePosition>> Handle(CreateEmployeePositionCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListEmployeePositionCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<EmployeePosition>.Success(result.Content![0])
            : Result<EmployeePosition>.Failure(result.Errors);
    }
}
