using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Interfaces.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class MediaOwnerWriteAccess(
    ICurrentIdentity actor,
    IIdentityRepositories identity,
    IIdentityUnitOfWork unitOfWork)
{
    public async Task<IIdentityTransaction> BeginAsync(CancellationToken ct)
    {
        var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            await identity.Users.LockAsync(actor.UserId, ct);
            await identity.Enterprises.LockAsync(actor.EnterpriseId, ct);

            if (!await identity.EnterpriseMemberships.ExistsAsync(Membership =>
                Membership.Id == actor.MembershipId && Membership.UserId == actor.UserId &&
                Membership.EnterpriseId == actor.EnterpriseId && Membership.KindId == 1 && Membership.IsActive &&
                !Membership.IsContractBlocked && Membership.User.IsActive && Membership.Enterprise.IsActive,
                ct, ignoreQueryFilters: true))
                throw new SecurityFailure(403, "Access.PrimaryOwnerRequired", "Apenas o dono principal pode gerir logos.");

            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();

            throw;
        }
    }
}
