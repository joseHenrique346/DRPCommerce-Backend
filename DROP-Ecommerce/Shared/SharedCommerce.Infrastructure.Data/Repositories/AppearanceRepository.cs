using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.StaticEntity;
using SharedCommerce.Infrastructure.Data.Context;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;

namespace SharedCommerce.Infrastructure.Data.Repositories;

public sealed class AppearanceRepository<TContext>(TContext db, AppearanceWriteScope scope, AppearanceChannel channel)
    : IAppearanceRepository where TContext : DbContext
{
    public AppearanceChannel Channel => channel;

    public async Task<IAppearanceTransaction> BeginTransactionAsync(long enterpriseId, CancellationToken ct)
    {
        var lease = scope.Begin(enterpriseId);
        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await db.Database.BeginTransactionAsync(ct);
            await LockTenantAsync(db, enterpriseId, ct);

            return new AppearanceTransaction(db, transaction, lease);
        }
        catch
        {
            if (transaction is not null)
                await transaction.DisposeAsync();

            lease.Dispose();

            throw;
        }
    }

    public static Task LockTenantAsync(DbContext context, long enterpriseId, CancellationToken ct)
    {
        var key = enterpriseId.ToString(CultureInfo.InvariantCulture);

        return context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(1268463, hashtext({key}))", ct);
    }

    public Task<AppearanceEntity?> GetAsync(long enterpriseId, InputAppearanceTarget target, CancellationToken ct)
    {
        return db.Set<AppearanceEntity>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(Appearance => Appearance.EnterpriseId == enterpriseId &&
                Appearance.AppearanceTargetTypeId == target.AppearanceTargetTypeId && Appearance.DropEventId == target.DropEventId, ct);
    }

    public Task<ThemeRelease?> GetReleaseAsync(string key, string version, CancellationToken ct)
    {
        return db.Set<ThemeRelease>().SingleOrDefaultAsync(ThemeRelease =>
            ThemeRelease.AppearanceChannelId == channel.Id && ThemeRelease.Key == key && ThemeRelease.Version == version, ct);
    }

    public Task<ThemeRelease> GetReleaseAsync(long id, CancellationToken ct)
    {
        return db.Set<ThemeRelease>().SingleAsync(ThemeRelease => ThemeRelease.Id == id && ThemeRelease.AppearanceChannelId == channel.Id, ct);
    }

    public async Task<IReadOnlyList<ThemeRelease>> GetReleasesAsync(CancellationToken ct)
    {
        return await db.Set<ThemeRelease>().Where(ThemeRelease => ThemeRelease.AppearanceChannelId == channel.Id)
            .OrderBy(ThemeRelease => ThemeRelease.Key).ThenBy(ThemeRelease => ThemeRelease.Id).ToListAsync(ct);
    }

    public Task<AppearanceEditSession?> GetOpenSessionAsync(long enterpriseId, long appearanceId, CancellationToken ct)
    {
        return db.Set<AppearanceEditSession>().IgnoreQueryFilters().SingleOrDefaultAsync(AppearanceEditSession =>
            AppearanceEditSession.EnterpriseId == enterpriseId && AppearanceEditSession.AppearanceId == appearanceId && AppearanceEditSession.IsOpen, ct);
    }

    public Task<AppearanceEditSession?> GetSessionAsync(long enterpriseId, long id, CancellationToken ct)
    {
        return db.Set<AppearanceEditSession>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(AppearanceEditSession => AppearanceEditSession.EnterpriseId == enterpriseId && AppearanceEditSession.Id == id, ct);
    }

    public Task<AppearanceRevision?> GetRevisionAsync(long enterpriseId, long id, CancellationToken ct)
    {
        return db.Set<AppearanceRevision>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(AppearanceRevision => AppearanceRevision.EnterpriseId == enterpriseId && AppearanceRevision.Id == id, ct);
    }

    public async Task<IReadOnlyList<AppearanceRevision>> GetRevisionsAsync(long enterpriseId, long appearanceId, CancellationToken ct, long? beforeNumber = null)
    {
        return await db.Set<AppearanceRevision>().IgnoreQueryFilters()
            .Where(AppearanceRevision => AppearanceRevision.EnterpriseId == enterpriseId && AppearanceRevision.AppearanceId == appearanceId)
            .Where(AppearanceRevision => beforeNumber == null || AppearanceRevision.Number < beforeNumber)
            .OrderByDescending(AppearanceRevision => AppearanceRevision.Number).Take(100).ToListAsync(ct);
    }

    public async Task<long> NextRevisionNumberAsync(long enterpriseId, long appearanceId, CancellationToken ct)
    {
        var number = await db.Set<AppearanceRevision>().IgnoreQueryFilters()
            .Where(AppearanceRevision => AppearanceRevision.EnterpriseId == enterpriseId && AppearanceRevision.AppearanceId == appearanceId)
            .MaxAsync(AppearanceRevision => (long?)AppearanceRevision.Number, ct);

        return (number ?? 0) + 1;
    }

    public void Add(AppearanceEntity appearance)
    {
        db.Set<AppearanceEntity>().Add(appearance);
    }

    public void Add(AppearanceEditSession session)
    {
        db.Set<AppearanceEditSession>().Add(session);
    }

    public void Add(AppearanceRevision revision)
    {
        db.Set<AppearanceRevision>().Add(revision);
    }

    public Task SaveAsync(CancellationToken ct)
    {
        return db.SaveChangesAsync(ct);
    }

    public void Add(ThemeRelease release)
    {
        db.Set<ThemeRelease>().Add(release);
    }

    public async Task<IReadOnlyList<(long EnterpriseId, InputAppearanceTarget Target)>> GetMigrationTargetsAsync(DateTime now, CancellationToken ct)
    {
        var targets = await (
            from Appearance in db.Set<AppearanceEntity>().IgnoreQueryFilters()
            join Revision in db.Set<AppearanceRevision>().IgnoreQueryFilters() on Appearance.PublishedRevisionId equals Revision.Id
            join Release in db.Set<ThemeRelease>() on Revision.ThemeReleaseId equals Release.Id
            where !Appearance.MigrationBlocked && Release.MigrationDeadline <= now && Release.ReplacementReleaseId != null
            orderby Appearance.Id
            select new { Appearance.EnterpriseId, Appearance.AppearanceTargetTypeId, Appearance.DropEventId }).Take(100).ToListAsync(ct);

        return targets.Select(Target => (Target.EnterpriseId,
            new InputAppearanceTarget(Target.AppearanceTargetTypeId, Target.DropEventId))).ToList();
    }

    private sealed class AppearanceTransaction(DbContext context, IDbContextTransaction transaction, IDisposable lease) : IAppearanceTransaction
    {
        private bool committed;

        public async Task CommitAsync(CancellationToken ct)
        {
            await transaction.CommitAsync(ct);
            committed = true;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await transaction.DisposeAsync();
            }
            finally
            {
                if (!committed)
                    context.ChangeTracker.Clear();

                lease.Dispose();
            }
        }
    }
}