using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Validation;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Infrastructure.Data.Context;

public static class AppearanceContractGuard
{
    public static async Task GuardAsync(DbContext db,
        Func<long, CancellationToken, Task<AppearanceCapabilities>> readRights, CancellationToken ct)
    {
        var checks = new List<(long Tenant, long Release, string Settings)>();

        foreach (var entry in db.ChangeTracker.Entries<AppearanceEditSession>().Where(Entry =>
            Entry.State == EntityState.Added || Entry.State == EntityState.Modified && Entry.Entity.IsOpen &&
                (Entry.Property(Session => Session.SettingsJson).IsModified || Entry.Property(Session => Session.PreviewJson).IsModified)))
            checks.Add((entry.Entity.EnterpriseId, entry.Entity.ThemeReleaseId, entry.Entity.SettingsJson));

        foreach (var entry in db.ChangeTracker.Entries<AppearanceRevision>().Where(Entry => Entry.State == EntityState.Added))
            checks.Add((entry.Entity.EnterpriseId, entry.Entity.ThemeReleaseId, entry.Entity.SettingsJson));

        foreach (var entry in db.ChangeTracker.Entries<Appearance>().Where(Entry =>
            Entry.State == EntityState.Modified && Entry.Property(Appearance => Appearance.PublishedRevisionId).IsModified))
        {
            var revision = db.ChangeTracker.Entries<AppearanceRevision>().Select(Entry => Entry.Entity)
                .FirstOrDefault(Revision => Revision.Id == entry.Entity.PublishedRevisionId && Revision.EnterpriseId == entry.Entity.EnterpriseId);
            revision ??= await db.Set<AppearanceRevision>().IgnoreQueryFilters().SingleOrDefaultAsync(Revision =>
                Revision.Id == entry.Entity.PublishedRevisionId && Revision.EnterpriseId == entry.Entity.EnterpriseId, ct);

            if (revision is not null)
                checks.Add((revision.EnterpriseId, revision.ThemeReleaseId, revision.SettingsJson));
        }

        foreach (var group in checks.GroupBy(Check => Check.Tenant).OrderBy(Group => Group.Key))
        {
            var rights = await readRights(group.Key, ct);

            foreach (var check in group.Distinct())
            {
                var release = await db.Set<ThemeRelease>().SingleOrDefaultAsync(Release => Release.Id == check.Release, ct)
                    ?? throw new AppearanceFailure(404, "Appearance.ThemeNotFound", "Tema indisponível.");
                AppearanceCommercialSettings.EnsureAllowed(release, check.Settings, rights);
            }
        }
    }
}
