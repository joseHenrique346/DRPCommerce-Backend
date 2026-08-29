using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("cliente");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.FullName).HasMaxLength(200);
        builder.Property(e => e.PasswordHash).HasMaxLength(500);
        builder.Property(e => e.AddressLine).HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.Gender).HasMaxLength(100);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.OwnsOne(e => e.Email, nav =>
        {
            nav.Property(p => p.Value).HasColumnName("Email").HasMaxLength(200).IsRequired();
        });

        builder.OwnsOne(e => e.Phone, nav =>
        {
            nav.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20).IsRequired();
        });

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListCustomer)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.State)
            .WithMany()
            .HasForeignKey(e => e.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListOrder)
            .WithOne(e => e.Customer)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListInvoice)
            .WithOne(e => e.Customer)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListTransaction)
            .WithOne(e => e.Customer)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.StateId);
    }
}
