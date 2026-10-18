using Commerce.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class DeleteDomainCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<DeleteDomainCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(DeleteDomainCommand command, CancellationToken ct)
    {
        var id = command.Id;

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var domain = await data.EnterpriseDomains.FindAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true) ?? throw NotFound();

        if (domain.IsPrimary)
            throw new SecurityFailure(409, "Domain.PrimaryProtected", "Escolha outro domínio primário antes de excluir.");

        data.EnterpriseDomains.Remove(domain);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
