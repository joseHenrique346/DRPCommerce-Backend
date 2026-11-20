using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class EnterpriseContractUsageReader(StoreCommerceDbContext context, TimeProvider time) : IEnterpriseContractUsageReader
{
    public async Task<EnterpriseContractUsage> ReadAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow().UtcDateTime;

        return await context.Enterprises.IgnoreQueryFilters().AsNoTracking()
            .Where(Enterprise => Enterprise.Id == enterpriseId && Enterprise.IsActive)
            .Select(Enterprise => new EnterpriseContractUsage(
                context.Products.IgnoreQueryFilters().LongCount(Product =>
                    Product.EnterpriseId == enterpriseId && !Product.IsDeleted),
                context.Products.IgnoreQueryFilters().LongCount(Product =>
                    Product.EnterpriseId == enterpriseId && !Product.IsDeleted && Product.IsActive),
                context.Products.IgnoreQueryFilters().LongCount(Product =>
                    Product.EnterpriseId == enterpriseId && !Product.IsDeleted && !Product.IsActive),
                context.EnterpriseMemberships.IgnoreQueryFilters().LongCount(Membership =>
                    Membership.EnterpriseId == enterpriseId && Membership.IsActive &&
                    Membership.KindId != 1 && Membership.User.IsActive),
                context.EnterpriseInvitations.IgnoreQueryFilters().LongCount(Invitation =>
                    Invitation.EnterpriseId == enterpriseId && Invitation.AcceptedAt == null &&
                    Invitation.RevokedAt == null && Invitation.ExpiresAt > now),
                null,
                false,
                context.Products.IgnoreQueryFilters().LongCount(Product =>
                    Product.EnterpriseId == enterpriseId && !Product.IsDeleted && Product.IsContractBlocked),
                context.Products.IgnoreQueryFilters().LongCount(Product =>
                    Product.EnterpriseId == enterpriseId && !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked)))
            .SingleAsync(cancellationToken);
    }
}
