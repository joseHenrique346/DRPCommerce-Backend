using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RefreshCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RefreshCommand, SessionResult>(context)
{
    protected override async Task<SessionResult> ExecuteAsync(RefreshCommand command, CancellationToken ct)
    {
        var token = command.Token;
        var customer = command.Customer;
        var membershipId = command.MembershipId;

        var hash = OpaqueToken.Hash(token);
        using var operation = scope.Begin();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

        if (customer)
        {
            var tenant = actor.EnterpriseId;
            var reference = await data.CustomerRefreshTokens.FindAsync(
                x => x.TokenHash == hash && x.EnterpriseId == tenant,
                ct,
                ignoreQueryFilters: true,
                tracking: false) ?? throw InvalidToken();
            await LockCustomerAsync(reference.CustomerId, ct);
            var current = await data.CustomerRefreshTokens.GetRequiredAsync(x => x.Id == reference.Id && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);
            var account = await data.Customers.GetRequiredAsync(x => x.Id == current.CustomerId && x.EnterpriseId == tenant, ct, ignoreQueryFilters: true);

            if (current.ConsumedAt is not null)
            {
                await RevokeFamilyAsync(account.Id, current.FamilyId, true, ct);
                await transaction.CommitAsync(ct);
                throw InvalidToken();
            }

            if (!current.IsUsable(Now, account.SecurityVersion) || !account.IsActive || account.IsDeleted ||
                (Options.RequireConfirmedEmail && !account.EmailConfirmed) ||
                await data.CustomerRefreshTokens.ExistsAsync(
                    x => x.CustomerId == account.Id && x.FamilyId == current.FamilyId && x.RevokedAt != null,
                    ct,
                    ignoreQueryFilters: true))
                throw InvalidToken();

            var raw = OpaqueToken.Create();
            var next = CustomerRefreshToken.Create(account, OpaqueToken.Hash(raw), current.FamilyId, current.FamilyExpiresAt);
            current.Consume(Now);
            data.CustomerRefreshTokens.Add(next);
            await unitOfWork.CommitAsync(ct);
            current.SetReplacement(next.Id);
            await unitOfWork.CommitAsync(ct);
            await transaction.CommitAsync(ct);
            var access = jwt.IssueCustomer(account, current.FamilyId);

            return new(access.Token, access.Expires, false, [], raw, next.ExpiresAt);
        }
        else
        {
            var reference = await data.UserRefreshTokens.FindAsync(x => x.TokenHash == hash, ct, tracking: false) ?? throw InvalidToken();
            await LockUserAsync(reference.UserId, ct);
            var current = await data.UserRefreshTokens.GetRequiredAsync(x => x.Id == reference.Id, ct);
            var account = await data.Users.GetRequiredAsync(x => x.Id == current.UserId, ct);

            if (current.ConsumedAt is not null)
            {
                await RevokeFamilyAsync(account.Id, current.FamilyId, false, ct);
                await transaction.CommitAsync(ct);
                throw InvalidToken();
            }

            if (!current.IsUsable(Now, account.SecurityVersion) || !account.IsActive || (Options.RequireConfirmedEmail && !account.EmailConfirmed) ||
                await data.UserRefreshTokens.ExistsAsync(x => x.UserId == account.Id && x.FamilyId == current.FamilyId && x.RevokedAt != null, ct))
                throw InvalidToken();

            var memberships = await ActiveMembershipsAsync(account.Id, ct);
            var chosen = membershipId ?? current.EnterpriseMembershipId;
            EnterpriseMembership? member = null;

            if (chosen.HasValue)
                member = memberships.SingleOrDefault(x => x.Id == chosen.Value) ??
                    throw new SecurityFailure(403, "Identity.MembershipUnavailable", "Acesso à loja indisponível.");
            else if (memberships.Count == 1)
                member = memberships[0];

            var raw = OpaqueToken.Create();
            var next = UserRefreshToken.Create(account, OpaqueToken.Hash(raw), current.FamilyId, current.FamilyExpiresAt, member?.Id);
            current.Consume(Now);
            data.UserRefreshTokens.Add(next);
            await unitOfWork.CommitAsync(ct);
            current.SetReplacement(next.Id);
            await unitOfWork.CommitAsync(ct);
            await transaction.CommitAsync(ct);

            return await UserSessionAsync(account, current.FamilyId, raw, next.ExpiresAt, member, memberships, ct);
        }
    }
}
