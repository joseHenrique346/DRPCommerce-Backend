using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("fornecedor");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CompanyName).HasMaxLength(200);
        builder.Property(e => e.ContactName).HasMaxLength(200);
        builder.Property(e => e.AddressLine).HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);

        builder.OwnsOne(e => e.Email, nav =>
        {
            nav.Property(p => p.Value).HasColumnName("Email").HasMaxLength(200).IsRequired();
        });

        builder.OwnsOne(e => e.Phone, nav =>
        {
            nav.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20).IsRequired();
        });

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListSupplier)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.State)
            .WithMany()
            .HasForeignKey(e => e.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListProduct)
            .WithOne(e => e.Supplier)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListShipment)
            .WithOne(e => e.Supplier)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.StateId);
        builder.HasIndex(e => e.CompanyName);
    }
}
