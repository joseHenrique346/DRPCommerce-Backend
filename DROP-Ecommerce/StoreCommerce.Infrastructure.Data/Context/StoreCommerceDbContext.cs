using SharedCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Inventory;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;
using AppearanceBaseEntity = SharedCommerce.Domain.Entity.Base.BaseEntity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly AppearanceWriteScope _appearanceWrites;
    private readonly IIdentityOperationScope _identityScope;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpAccessor;
    private readonly InventoryWriteScope _inventoryWrites;

    public StoreCommerceDbContext(DbContextOptions<StoreCommerceDbContext> options, ITenantProvider tenantProvider,
        IIdentityOperationScope identityScope, Microsoft.AspNetCore.Http.IHttpContextAccessor httpAccessor, AppearanceWriteScope appearanceWrites,
        InventoryWriteScope inventoryWrites)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _appearanceWrites = appearanceWrites;
        _identityScope = identityScope;
        _httpAccessor = httpAccessor;
        _inventoryWrites = inventoryWrites;
    }

    #region DbSets

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<StockReservationLine> StockReservationLines => Set<StockReservationLine>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Enterprise> Enterprises => Set<Enterprise>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<EmployeePosition> EmployeePositions => Set<EmployeePosition>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EnterpriseMembership> EnterpriseMemberships => Set<EnterpriseMembership>();
    public DbSet<EnterpriseDomain> EnterpriseDomains => Set<EnterpriseDomain>();
    public DbSet<AccessRole> AccessRoles => Set<AccessRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<AccessRolePermission> AccessRolePermissions => Set<AccessRolePermission>();
    public DbSet<MembershipAccessRole> MembershipAccessRoles => Set<MembershipAccessRole>();
    public DbSet<EnterpriseMembershipKind> EnterpriseMembershipKinds => Set<EnterpriseMembershipKind>();
    public DbSet<StoreSettings> StoreSettings => Set<StoreSettings>();

    // Static Entities
    public DbSet<CouponType> CouponTypes => Set<CouponType>();
    public DbSet<CouponConditionType> CouponConditionTypes => Set<CouponConditionType>();
    public DbSet<CouponConditionOperator> CouponConditionOperators => Set<CouponConditionOperator>();
    public DbSet<DocumentStatus> DocumentStatuses => Set<DocumentStatus>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<InvoiceStatus> InvoiceStatuses => Set<InvoiceStatus>();
    public DbSet<InvoiceType> InvoiceTypes => Set<InvoiceType>();
    public DbSet<OrderPaymentStatus> OrderPaymentStatuses => Set<OrderPaymentStatus>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<ShipmentStatus> ShipmentStatuses => Set<ShipmentStatus>();
    public DbSet<ShipmentType> ShipmentTypes => Set<ShipmentType>();
    public DbSet<State> States => Set<State>();
    public DbSet<TransactionMethod> TransactionMethods => Set<TransactionMethod>();
    public DbSet<TransactionStatus> TransactionStatuses => Set<TransactionStatus>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureAppearance(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreCommerceDbContext).Assembly);
        ConfigureSecurityFilters(modelBuilder);

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

        // Entities with BOTH ITenantEntity + ISoftDeletable
        ApplyTenantAndSoftDeleteFilter<Product>(modelBuilder);
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(ProductVariant =>
            ProductVariant.EnterpriseId == _tenantProvider.GetEnterpriseId() && !ProductVariant.IsDeleted && !ProductVariant.Product.IsDeleted);
        modelBuilder.Entity<StockReservation>().HasQueryFilter(StockReservation => StockReservation.EnterpriseId == _tenantProvider.GetEnterpriseId());
        modelBuilder.Entity<StockReservationLine>().HasQueryFilter(StockReservationLine => StockReservationLine.EnterpriseId == _tenantProvider.GetEnterpriseId());
        modelBuilder.Entity<StockMovement>().HasQueryFilter(StockMovement => StockMovement.EnterpriseId == _tenantProvider.GetEnterpriseId());
        ApplyTenantAndSoftDeleteFilter<Order>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Customer>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Coupon>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Service>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Category>(modelBuilder);

        // Entities with ITenantEntity ONLY
        ApplyTenantFilter<Employee>(modelBuilder);
        ApplyTenantFilter<EmployeePosition>(modelBuilder);
        ApplyTenantFilter<Department>(modelBuilder);
        ApplyTenantFilter<EnterpriseMembership>(modelBuilder);
        ApplyTenantFilter<EnterpriseDomain>(modelBuilder);
        ApplyTenantFilter<AccessRole>(modelBuilder);
        modelBuilder.Entity<AccessRolePermission>().HasQueryFilter(e => e.EnterpriseId == _tenantProvider.GetEnterpriseId());
        modelBuilder.Entity<MembershipAccessRole>().HasQueryFilter(e => e.EnterpriseId == _tenantProvider.GetEnterpriseId());
        ApplyTenantFilter<Invoice>(modelBuilder);
        ApplyTenantFilter<Document>(modelBuilder);
        ApplyTenantFilter<Supplier>(modelBuilder);
        ApplyTenantFilter<CouponRedemption>(modelBuilder);
        ApplyTenantFilter<StoreSettings>(modelBuilder);

        // Dependents of filtered aggregates must use the same visibility rules.
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e =>
            e.Order.EnterpriseId == _tenantProvider.GetEnterpriseId() && !e.Order.IsDeleted);
        modelBuilder.Entity<Shipment>().HasQueryFilter(e =>
            e.Order.EnterpriseId == _tenantProvider.GetEnterpriseId() && !e.Order.IsDeleted);
        modelBuilder.Entity<Transaction>().HasQueryFilter(e =>
            e.Customer.EnterpriseId == _tenantProvider.GetEnterpriseId() && !e.Customer.IsDeleted);
    }

    private void ApplyTenantAndSoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : BaseEntity, ITenantEntity, ISoftDeletable
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            e.EnterpriseId == _tenantProvider.GetEnterpriseId() && !e.IsDeleted);
    }

    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder)
        where T : BaseEntity, ITenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            e.EnterpriseId == _tenantProvider.GetEnterpriseId());
    }

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        _appearanceWrites.Guard(this);
        GuardWritesAsync(CancellationToken.None).GetAwaiter().GetResult();

        UpdateChangedDates();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(true, cancellationToken);

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        _appearanceWrites.Guard(this);
        await GuardWritesAsync(cancellationToken);

        UpdateChangedDates();

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateChangedDates()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.SetChangedDate();
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
