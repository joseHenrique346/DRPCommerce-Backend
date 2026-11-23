using MediatR;
using SharedCommerce.Application.Result;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CreateEnterpriseDomainCommandHandler(
    IEnterpriseDomainRepository repository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : IRequestHandler<CreateEnterpriseDomainCommand, Result<EnterpriseDomain>>
{
    public async Task<Result<EnterpriseDomain>> Handle(CreateEnterpriseDomainCommand request, CancellationToken ct)
    {
        var entity = EnterpriseDomain.Create(tenantProvider.GetEnterpriseId(), request.host, request.isPrimary, request.isActive);
        await repository.AddRangeAsync([entity], ct);
        await unitOfWork.CommitAsync(ct);

        return Result<EnterpriseDomain>.Success(entity);
    }
}
