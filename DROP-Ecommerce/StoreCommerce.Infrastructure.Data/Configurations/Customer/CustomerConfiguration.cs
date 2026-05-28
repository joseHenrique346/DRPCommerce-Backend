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
        builder.Property(e => e.State).HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.Gender).HasMaxLength(100);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(e => e.EnterpriseId);

        builder.HasOne<Enterprise>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
