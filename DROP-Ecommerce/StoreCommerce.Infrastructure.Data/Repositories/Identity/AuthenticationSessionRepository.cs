using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Identity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class AuthenticationSessionRepository(StoreCommerceDbContext context) : IAuthenticationSessionRepository
{
    public Task RevokeFamilyAsync(long accountId, Guid familyId, bool customer, DateTime revokedAt, CancellationToken cancellationToken)
    {
        if (customer)
            return context.CustomerRefreshTokens.IgnoreQueryFilters()
                .Where(Token => Token.CustomerId == accountId && Token.FamilyId == familyId)
                .ExecuteUpdateAsync(Update => Update.SetProperty(Token => Token.RevokedAt, revokedAt), cancellationToken);

        return context.UserRefreshTokens
            .Where(Token => Token.UserId == accountId && Token.FamilyId == familyId)
            .ExecuteUpdateAsync(Update => Update.SetProperty(Token => Token.RevokedAt, revokedAt), cancellationToken);
    }

    public Task RevokeAllAsync(long accountId, bool customer, DateTime revokedAt, CancellationToken cancellationToken)
    {
        if (customer)
            return context.CustomerRefreshTokens.IgnoreQueryFilters()
                .Where(Token => Token.CustomerId == accountId && Token.RevokedAt == null)
                .ExecuteUpdateAsync(Update => Update.SetProperty(Token => Token.RevokedAt, revokedAt), cancellationToken);

        return context.UserRefreshTokens
            .Where(Token => Token.UserId == accountId && Token.RevokedAt == null)
            .ExecuteUpdateAsync(Update => Update.SetProperty(Token => Token.RevokedAt, revokedAt), cancellationToken);
    }

    public async Task<IReadOnlyList<SessionDescription>> ListAsync(long accountId, Guid currentFamilyId, bool customer, long? enterpriseId, DateTime now, CancellationToken cancellationToken)
    {
        if (customer)
            return await context.CustomerRefreshTokens.IgnoreQueryFilters()
                .Where(Token => Token.CustomerId == accountId && Token.EnterpriseId == enterpriseId &&
                    Token.RevokedAt == null && Token.FamilyExpiresAt > now)
                .GroupBy(Token => Token.FamilyId)
                .Select(Family => new SessionDescription(
                    Family.Key,
                    Family.Min(Token => Token.CreatedAt),
                    Family.Max(Token => Token.FamilyExpiresAt),
                    Family.Key == currentFamilyId))
                .ToListAsync(cancellationToken);

        return await context.UserRefreshTokens
            .Where(Token => Token.UserId == accountId && Token.RevokedAt == null && Token.FamilyExpiresAt > now)
            .GroupBy(Token => Token.FamilyId)
            .Select(Family => new SessionDescription(
                Family.Key,
                Family.Min(Token => Token.CreatedAt),
                Family.Max(Token => Token.FamilyExpiresAt),
                Family.Key == currentFamilyId))
            .ToListAsync(cancellationToken);
    }

    public async Task CleanupAsync(DateTime before, CancellationToken cancellationToken)
    {
        // Consumed tokens remain until after the entire family's absolute expiry.
        await context.UserRefreshTokens.Where(Token => Token.FamilyExpiresAt < before).ExecuteDeleteAsync(cancellationToken);
        await context.CustomerRefreshTokens.IgnoreQueryFilters().Where(Token => Token.FamilyExpiresAt < before).ExecuteDeleteAsync(cancellationToken);
        await context.UserActionTokens.Where(Token => Token.ExpiresAt < before).ExecuteDeleteAsync(cancellationToken);
        await context.CustomerActionTokens.IgnoreQueryFilters().Where(Token => Token.ExpiresAt < before).ExecuteDeleteAsync(cancellationToken);
        await context.EnterpriseInvitations.IgnoreQueryFilters().Where(Invitation => Invitation.ExpiresAt < before).ExecuteDeleteAsync(cancellationToken);
        await context.OwnershipTransfers.IgnoreQueryFilters().Where(Transfer => Transfer.ExpiresAt < before).ExecuteDeleteAsync(cancellationToken);
    }
}
