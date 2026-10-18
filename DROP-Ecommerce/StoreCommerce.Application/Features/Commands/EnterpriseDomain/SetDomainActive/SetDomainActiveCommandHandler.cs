using Commerce.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetDomainActiveCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<SetDomainActiveCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(SetDomainActiveCommand command, CancellationToken ct)
    {
        var id = command.Id;
        var request = command.Request;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var domain = await data.EnterpriseDomains.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();

        if (request.IsPrimary && !request.IsActive)
            throw new SecurityFailure(400, "Domain.PrimaryMustBeActive", "Domínio primário precisa estar ativo.");

        if (domain.IsPrimary && (!request.IsPrimary || !request.IsActive))
            throw new SecurityFailure(409, "Domain.PrimaryProtected", "Escolha outro domínio primário antes de desativar ou remover este domínio primário.");

        if (request.IsPrimary)
        {
            foreach (var other in await data.EnterpriseDomains.ListAsync(x => x.EnterpriseId == tenant && x.Id != id && x.IsPrimary, ct, ignoreQueryFilters: true))
                other.SetPublication(false, other.IsActive);

            await unitOfWork.CommitAsync(ct);
        }

        domain.SetPublication(request.IsPrimary, request.IsActive);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
