using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropProductConfiguration : IEntityTypeConfiguration<DropProduct>
{
    public void Configure(EntityTypeBuilder<DropProduct> builder)
    {
        builder.ToTable("produto");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.SKU).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListDropProduct).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropOrderItem).WithOne(e => e.DropProduct).HasForeignKey(e => e.DropProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropReservation).WithOne(e => e.DropProduct).HasForeignKey(e => e.DropProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListWaitlistEntry).WithOne(e => e.DropProduct).HasForeignKey(e => e.DropProductId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.ProductId);
    }
}
