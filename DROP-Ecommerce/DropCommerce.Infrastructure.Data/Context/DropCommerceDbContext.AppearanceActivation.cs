using Commerce.Appearance.Domain;
using Commerce.Appearance.Domain.StaticEntity;
using Commerce.Appearance.Infrastructure;
using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using AppearanceEntity = Commerce.Appearance.Domain.Appearance;

namespace DropCommerce.Infrastructure.Data.Context;

public partial class DropCommerceDbContext
{
    private async Task<int> SaveWithAppearanceActivationAsync(bool acceptAllChangesOnSuccess, CancellationToken ct)
    {
        var activated = ChangeTracker.Entries<DropEvent>()
            .Where(Entry => Entry.State is EntityState.Added or EntityState.Modified &&
                Entry.Entity.EffectiveAppearanceRevisionId == null && Entry.Entity.DropEventStatusId is >= 5 and <= 7 &&
                (Entry.State == EntityState.Added || Entry.Property(DropEvent => DropEvent.DropEventStatusId).OriginalValue is < 5 or > 7))
            .Select(Entry => Entry.Entity).ToList();
        IDbContextTransaction? transaction = null;

        try
        {
            if (activated.Count > 0 && Database.CurrentTransaction is null)
                transaction = await Database.BeginTransactionAsync(ct);

            foreach (var tenant in activated.Select(DropEvent => DropEvent.EnterpriseId).Distinct().Order())
                await EfAppearanceRepository<DropCommerceDbContext>.LockTenantAsync(this, tenant, ct);

            foreach (var dropEvent in activated)
            {
                var own = await Set<AppearanceEntity>().IgnoreQueryFilters()
                    .Where(Appearance => Appearance.EnterpriseId == dropEvent.EnterpriseId &&
                        Appearance.AppearanceTargetTypeId == AppearanceTargetType.DropSpecific.Id && Appearance.DropEventId == dropEvent.Id)
                    .Select(Appearance => Appearance.PublishedRevisionId).SingleOrDefaultAsync(ct);
                var fallback = own ?? await Set<AppearanceEntity>().IgnoreQueryFilters()
                    .Where(Appearance => Appearance.EnterpriseId == dropEvent.EnterpriseId &&
                        Appearance.AppearanceTargetTypeId == AppearanceTargetType.DropDefault.Id && Appearance.DropEventId == null)
                    .Select(Appearance => Appearance.PublishedRevisionId).SingleOrDefaultAsync(ct);

                if (fallback.HasValue)
                    dropEvent.PinAppearance(fallback.Value);
            }

            UpdateChangedDates();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);

            if (transaction is not null)
                await transaction.CommitAsync(ct);

            return result;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }
}
