using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("historico_status_pedido");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Source).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Reason).HasMaxLength(OrderStatusHistory.ReasonMaxLength);

        builder.HasOne<OrderStatus>().WithMany().HasForeignKey(e => e.FromStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OrderStatus>().WithMany().HasForeignKey(e => e.ToStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OrderPaymentStatus>().WithMany().HasForeignKey(e => e.FromPaymentStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<OrderPaymentStatus>().WithMany().HasForeignKey(e => e.ToPaymentStatusId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => new { e.OrderId, e.ChangedAt });
    }
}
