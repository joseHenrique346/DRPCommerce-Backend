using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Context;

public class DropCommerceDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public DropCommerceDbContext(DbContextOptions<DropCommerceDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    #region DbSets

    public DbSet<DropEvent> DropEvents => Set<DropEvent>();
    public DbSet<DropProduct> DropProducts => Set<DropProduct>();
    public DbSet<DropCoupon> DropCoupons => Set<DropCoupon>();
    public DbSet<DropOrder> DropOrders => Set<DropOrder>();
    public DbSet<DropOrderItem> DropOrderItems => Set<DropOrderItem>();
    public DbSet<DropReservation> DropReservations => Set<DropReservation>();
    public DbSet<DropRegistration> DropRegistrations => Set<DropRegistration>();
    public DbSet<DropNotification> DropNotifications => Set<DropNotification>();
    public DbSet<DropTransaction> DropTransactions => Set<DropTransaction>();
    public DbSet<DropAuditLog> DropAuditLogs => Set<DropAuditLog>();
    public DbSet<FraudSignal> FraudSignals => Set<FraudSignal>();
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();
    public DbSet<QueueSession> QueueSessions => Set<QueueSession>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    // Static Entities
    public DbSet<DropEventStatus> DropEventStatuses => Set<DropEventStatus>();
    public DbSet<DropCouponType> DropCouponTypes => Set<DropCouponType>();
    public DbSet<DropOrderStatus> DropOrderStatuses => Set<DropOrderStatus>();
    public DbSet<DropOrderPaymentStatus> DropOrderPaymentStatuses => Set<DropOrderPaymentStatus>();
    public DbSet<DropNotificationType> DropNotificationTypes => Set<DropNotificationType>();
    public DbSet<DropNotificationStatus> DropNotificationStatuses => Set<DropNotificationStatus>();
    public DbSet<DropNotificationChannel> DropNotificationChannels => Set<DropNotificationChannel>();
    public DbSet<DropReservationStatus> DropReservationStatuses => Set<DropReservationStatus>();
    public DbSet<DropRegistrationStatus> DropRegistrationStatuses => Set<DropRegistrationStatus>();
    public DbSet<DropTransactionType> DropTransactionTypes => Set<DropTransactionType>();
    public DbSet<DropTransactionStatus> DropTransactionStatuses => Set<DropTransactionStatus>();
    public DbSet<DropTransactionMethod> DropTransactionMethods => Set<DropTransactionMethod>();
    public DbSet<FraudSignalType> FraudSignalTypes => Set<FraudSignalType>();
    public DbSet<FraudSeverity> FraudSeverities => Set<FraudSeverity>();
    public DbSet<QueueEntryStatus> QueueEntryStatuses => Set<QueueEntryStatus>();
    public DbSet<QueueSessionStatus> QueueSessionStatuses => Set<QueueSessionStatus>();
    public DbSet<WaitlistEntryStatus> WaitlistEntryStatuses => Set<WaitlistEntryStatus>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DropCommerceDbContext).Assembly);

        // Convention: snake_case for all column names
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnName()!));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()!));
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()!));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }

        // Global query filter: DropEvent has tenant + soft delete
        modelBuilder.Entity<DropEvent>().HasQueryFilter(e =>
            e.EnterpriseId == _tenantProvider.GetEnterpriseId() && !e.IsDeleted);

        // Global query filters for other ISoftDeletable entities
        ApplySoftDeleteFilter<DropProduct>(modelBuilder);
        ApplySoftDeleteFilter<DropCoupon>(modelBuilder);
        ApplySoftDeleteFilter<DropOrder>(modelBuilder);
        ApplySoftDeleteFilter<DropReservation>(modelBuilder);
        ApplySoftDeleteFilter<DropRegistration>(modelBuilder);
        ApplySoftDeleteFilter<DropNotification>(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity, ISoftDeletable
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetChangedDate();
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])
                ? "_" + c
                : c.ToString())).ToLower();
    }
}
