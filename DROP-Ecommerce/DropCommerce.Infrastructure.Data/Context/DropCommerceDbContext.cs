using DropCommerce.Application.Interfaces.Identity;
using SharedCommerce.Domain.Interfaces.Persistence;
using SharedCommerce.Domain.Interfaces.Tenancy;
using SharedCommerce.Infrastructure.Data.Context;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Application.Security;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.StaticEntity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AppearanceBaseEntity = SharedCommerce.Domain.Entity.Base.BaseEntity;

namespace DropCommerce.Infrastructure.Data.Context;

public partial class DropCommerceDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly AppearanceWriteScope _appearanceWrites;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpAccessor;
    private readonly IDropOperationScope _operationScope;
    private readonly IStoreCustomerReader _storeCustomers;
    private readonly IStoreProductReader _storeProducts;
    private readonly IStoreAppearanceReader _storeAppearance;
    private readonly IStoreCommercialReader _commercialReader;
    private readonly DropCommerce.Application.Contracts.DropCommercialWriteScope _commercialWrites;

    public DropCommerceDbContext(
        DbContextOptions<DropCommerceDbContext> options,
        ITenantProvider tenantProvider,
        IHttpContextAccessor httpAccessor,
        IDropOperationScope operationScope,
        IStoreCustomerReader storeCustomers,
        IStoreProductReader storeProducts,
        AppearanceWriteScope appearanceWrites,
        IStoreAppearanceReader storeAppearance,
        IStoreCommercialReader commercialReader,
        DropCommerce.Application.Contracts.DropCommercialWriteScope commercialWrites)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _appearanceWrites = appearanceWrites;
        _httpAccessor = httpAccessor;
        _operationScope = operationScope;
        _storeCustomers = storeCustomers;
        _storeProducts = storeProducts;
        _storeAppearance = storeAppearance;
        _commercialReader = commercialReader;
        _commercialWrites = commercialWrites;
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
        ConfigureAppearance(modelBuilder);
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
        modelBuilder.Entity<DropEvent>().HasQueryFilter(DropEvent =>
            DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            !DropEvent.IsDeleted);

        // Global query filters for other ISoftDeletable entities
        ApplySoftDeleteFilter<DropProduct>(modelBuilder);
        ApplySoftDeleteFilter<DropCoupon>(modelBuilder);
        ApplySoftDeleteFilter<DropOrder>(modelBuilder);
        ApplySoftDeleteFilter<DropReservation>(modelBuilder);
        ApplySoftDeleteFilter<DropRegistration>(modelBuilder);
        ApplySoftDeleteFilter<DropNotification>(modelBuilder);
        ConfigureSecurityFilters(modelBuilder);
        modelBuilder.Entity<DropContractState>().HasQueryFilter(DropContractState => DropContractState.EnterpriseId == _tenantProvider.GetEnterpriseId());
        modelBuilder.Entity<DropCommercialActivity>().HasQueryFilter(DropCommercialActivity => DropCommercialActivity.EnterpriseId == _tenantProvider.GetEnterpriseId());
        modelBuilder.Entity<DropCatalogProduct>().HasQueryFilter(DropCatalogProduct => DropCatalogProduct.EnterpriseId == _tenantProvider.GetEnterpriseId() && !DropCatalogProduct.IsDeleted);
        modelBuilder.Entity<DropCatalogVariant>().HasQueryFilter(DropCatalogVariant => DropCatalogVariant.CatalogProduct.EnterpriseId == _tenantProvider.GetEnterpriseId() && !DropCatalogVariant.CatalogProduct.IsDeleted);
        modelBuilder.Entity<DropStockImportReceipt>().HasQueryFilter(DropStockImportReceipt => DropStockImportReceipt.EnterpriseId == _tenantProvider.GetEnterpriseId());
        modelBuilder.Entity<DropStockReturnIntent>().HasQueryFilter(DropStockReturnIntent => DropStockReturnIntent.EnterpriseId == _tenantProvider.GetEnterpriseId());
        modelBuilder.Entity<DropReservationLine>().HasQueryFilter(DropReservationLine =>
            DropReservationLine.Reservation.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || DropReservationLine.Reservation.CustomerId == CurrentCustomerId));
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : BaseEntity, ISoftDeletable
    {
        modelBuilder.Entity<T>().HasQueryFilter(Entity => !Entity.IsDeleted);
    }

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return SaveChangesAsync(acceptAllChangesOnSuccess, CancellationToken.None).GetAwaiter().GetResult();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        await using var commercialTransaction = Database.CurrentTransaction is null && ChangeTracker.HasChanges()
            ? await Database.BeginTransactionAsync(cancellationToken) : null;
        await GuardCommercialContractsAsync(cancellationToken);
        _appearanceWrites.Guard(this);
        await AppearanceContractGuard.GuardAsync(this, _storeAppearance.RightsAsync, cancellationToken);
        await GuardWritesAsync(cancellationToken);

        var affected = await SaveWithAppearanceActivationAsync(acceptAllChangesOnSuccess, cancellationToken);
        _commercialGenerated.Clear();

        if (commercialTransaction is not null)
            await commercialTransaction.CommitAsync(cancellationToken);

        return affected;
    }

    private void UpdateChangedDates()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetChangedDate();
            }
        }

        foreach (var entry in ChangeTracker.Entries<AppearanceBaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetChangedDate();
        }
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])
                ? "_" + c
                : c.ToString())).ToLower();
    }
}
