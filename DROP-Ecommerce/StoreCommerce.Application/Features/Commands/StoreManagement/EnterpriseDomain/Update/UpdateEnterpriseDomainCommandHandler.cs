using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class UpdateEnterpriseDomainCommandHandler(
    IEnterpriseDomainRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEnterpriseDomainCommand, Result<EnterpriseDomain>>
{
    public async Task<Result<EnterpriseDomain>> Handle(UpdateEnterpriseDomainCommand request, CancellationToken ct)
    {
        var entity = (await repository.GetListByListIdAsync([request.id], ct)).SingleOrDefault();

        if (entity is null)
            return Result<EnterpriseDomain>.NotFound("EnterpriseDomain.NotFound", $"EnterpriseDomain com id {request.id} não encontrado.");

        entity.UpdateDetails(request.host, request.isPrimary, request.isActive);
        repository.UpdateRange([entity]);
        await unitOfWork.CommitAsync(ct);

        return Result<EnterpriseDomain>.Success(entity);
    }
}
