using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateAccessRoleCommandHandler(
    IAccessRoleRepository repository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : IRequestHandler<CreateAccessRoleCommand, Result<AccessRole>>
{
    public async Task<Result<AccessRole>> Handle(CreateAccessRoleCommand request, CancellationToken ct)
    {
        var entity = AccessRole.Create(tenantProvider.GetEnterpriseId(), request.name, request.description, request.isActive);
        await repository.AddRangeAsync([entity], ct);
        await unitOfWork.CommitAsync(ct);

        return Result<AccessRole>.Success(entity);
    }
}
