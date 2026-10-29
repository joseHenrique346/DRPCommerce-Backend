using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("movimentacao_estoque");
        builder.HasKey(Movement => Movement.Id);
        builder.Property(Movement => Movement.Id).ValueGeneratedOnAdd();
        builder.Property(Movement => Movement.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(Movement => Movement.OperationKey).HasMaxLength(100).IsRequired();
        builder.Property(Movement => Movement.Reason).HasMaxLength(500).IsRequired();
        builder.Property(Movement => Movement.SKU).HasMaxLength(Product.SkuMaxLength).IsRequired();
        builder.HasOne(Movement => Movement.ProductVariant).WithMany()
            .HasForeignKey(Movement => new { Movement.EnterpriseId, Movement.ProductVariantId })
            .HasPrincipalKey(Variant => new { Variant.EnterpriseId, Variant.Id }).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(Movement => Movement.StockReservation).WithMany()
            .HasForeignKey(Movement => new { Movement.EnterpriseId, Movement.StockReservationId })
            .HasPrincipalKey(Reservation => new { Reservation.EnterpriseId, Reservation.Id }).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(Movement => new { Movement.EnterpriseId, Movement.ProductVariantId, Movement.Type, Movement.OperationKey }).IsUnique();
        builder.HasIndex(Movement => new { Movement.EnterpriseId, Movement.OccurredAt });
    }
}
