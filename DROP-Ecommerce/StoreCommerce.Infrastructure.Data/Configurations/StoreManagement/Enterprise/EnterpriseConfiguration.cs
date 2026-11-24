using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;
using SharedCommerce.Domain.Contracts.Storefront;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EnterpriseConfiguration : IEntityTypeConfiguration<Enterprise>
{
    public void Configure(EntityTypeBuilder<Enterprise> builder)
    {
        builder.ToTable("empresa");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TradeName).HasMaxLength(200);
        builder.Property(e => e.LegalName).HasMaxLength(200);
        builder.Property(e => e.AddressLine).HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(Enterprise => Enterprise.StorefrontSlug).HasMaxLength(StorefrontSlug.MaxLength);
        builder.HasIndex(Enterprise => Enterprise.StorefrontSlug).IsUnique();

        builder.Property(Enterprise => Enterprise.PlanId).HasDefaultValue(1L);
        builder.Property(Enterprise => Enterprise.ContractRevision).HasDefaultValue(1L).IsConcurrencyToken();
        builder.Property(Enterprise => Enterprise.CatalogVersion).HasDefaultValue(1);
        builder.HasOne<StoreCommerce.Domain.StaticEntity.EnterpriseAddOn>()
            .WithMany()
            .HasForeignKey(Enterprise => Enterprise.AddOnId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StoreCommerce.Domain.StaticEntity.EnterprisePlanAddOn>()
            .WithMany()
            .HasForeignKey(Enterprise => new { Enterprise.PlanId, Enterprise.AddOnId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StoreCommerce.Domain.StaticEntity.EnterprisePlan>()
            .WithMany()
            .HasForeignKey(Enterprise => Enterprise.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(e => e.Email, nav =>
        {
            nav.Property<long>("EnterpriseId").HasColumnName("id");
            nav.Property(p => p.Value).HasColumnName("Email").HasMaxLength(200).IsRequired();
        });
        builder.OwnsOne(e => e.Phone, nav =>
        {
            nav.Property<long>("EnterpriseId").HasColumnName("id");
            nav.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(200).IsRequired();
        });

        builder.HasOne(e => e.State)
            .WithMany()
            .HasForeignKey(e => e.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListCategory)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListCoupon)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListCustomer)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListDocument)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListEmployee)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListInvoice)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListOrder)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListProduct)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListService)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListSupplier)
            .WithOne(e => e.Enterprise)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.StateId);
    }
}
