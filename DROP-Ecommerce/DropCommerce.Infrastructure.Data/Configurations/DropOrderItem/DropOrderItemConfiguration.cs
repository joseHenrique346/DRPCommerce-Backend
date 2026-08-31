using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropOrderItemConfiguration : IEntityTypeConfiguration<DropOrderItem>
{
    public void Configure(EntityTypeBuilder<DropOrderItem> builder)
    {
        builder.ToTable("item_pedido");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.SKU).IsRequired().HasMaxLength(100);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TotalPrice).HasPrecision(18, 2);

        builder.HasOne(e => e.DropOrder).WithMany(e => e.ListDropOrderItem).HasForeignKey(e => e.DropOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropProduct).WithMany(e => e.ListDropOrderItem).HasForeignKey(e => e.DropProductId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropOrderId);
        builder.HasIndex(e => e.DropProductId);
    }
}
