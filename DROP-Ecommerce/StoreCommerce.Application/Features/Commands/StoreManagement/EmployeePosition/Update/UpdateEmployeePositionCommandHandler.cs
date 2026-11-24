using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateEmployeePositionCommandHandler(
    IEmployeePositionRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEmployeePositionCommand, Result<EmployeePosition>>
{
    public async Task<Result<EmployeePosition>> Handle(UpdateEmployeePositionCommand request, CancellationToken ct)
    {
        var entity = (await repository.GetListByListIdAsync([request.id], ct)).SingleOrDefault();

        if (entity is null)
            return Result<EmployeePosition>.NotFound("EmployeePosition.NotFound", $"EmployeePosition com id {request.id} não encontrado.");

        entity.UpdateDetails(request.name, request.description, request.isActive);
        repository.UpdateRange([entity]);
        await unitOfWork.CommitAsync(ct);

        return Result<EmployeePosition>.Success(entity);
    }
}
