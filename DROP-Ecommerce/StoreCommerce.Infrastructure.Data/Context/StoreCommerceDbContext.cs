using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Entity.Category;
using StoreCommerce.Domain.Entity.Coupon;
using StoreCommerce.Domain.Entity.Product;
using StoreCommerce.Domain.Entity.Service;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Context;

public class StoreCommerceDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public StoreCommerceDbContext(DbContextOptions<StoreCommerceDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    #region DbSets

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Enterprise> Enterprises => Set<Enterprise>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Department> Departments => Set<Department>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreCommerceDbContext).Assembly);

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
        ApplyTenantAndSoftDeleteFilter<Order>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Customer>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Coupon>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Service>(modelBuilder);
        ApplyTenantAndSoftDeleteFilter<Category>(modelBuilder);

        // Entities with ITenantEntity ONLY
        ApplyTenantFilter<Employee>(modelBuilder);
        ApplyTenantFilter<Invoice>(modelBuilder);
        ApplyTenantFilter<Document>(modelBuilder);
        ApplyTenantFilter<Supplier>(modelBuilder);
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
