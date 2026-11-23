using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateEmployeePositionCommandHandler(
    IEmployeePositionRepository repository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : IRequestHandler<CreateEmployeePositionCommand, Result<EmployeePosition>>
{
    public async Task<Result<EmployeePosition>> Handle(CreateEmployeePositionCommand request, CancellationToken ct)
    {
        var entity = EmployeePosition.Create(tenantProvider.GetEnterpriseId(), request.name, request.description, request.isActive);
        await repository.AddRangeAsync([entity], ct);
        await unitOfWork.CommitAsync(ct);

        return Result<EmployeePosition>.Success(entity);
    }
}
