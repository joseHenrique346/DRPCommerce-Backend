using Commerce.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class VerifyDomainCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<VerifyDomainCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(VerifyDomainCommand command, CancellationToken ct)
    {
        var id = command.Id;

        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var reference = await data.EnterpriseDomains.FindAsync(
            x => x.Id == id && x.EnterpriseId == tenant,
            ct,
            ignoreQueryFilters: true,
            tracking: false) ?? throw NotFound();

        if (reference.IsVerified)
            return true;

        if (reference.VerificationTokenHash is null || !await domainVerifier.VerifyAsync(reference.NormalizedHost, reference.VerificationTokenHash, ct))
            throw new SecurityFailure(400, "Domain.OwnershipUnverified", "Registro TXT de verificação não encontrado.");

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);
        var domain = await data.EnterpriseDomains.GetRequiredAsync(x => x.Id == id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);

        if (domain.NormalizedHost != reference.NormalizedHost || domain.VerificationTokenHash != reference.VerificationTokenHash)
            throw new SecurityFailure(409, "Domain.ChallengeChanged", "O desafio de domínio foi alterado. Verifique novamente.");

        domain.Verify();
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }
}
