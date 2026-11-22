using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class StockReservationLineConfiguration : IEntityTypeConfiguration<StockReservationLine>
{
    public void Configure(EntityTypeBuilder<StockReservationLine> builder)
    {
        builder.ToTable("item_reserva_estoque", Table => Table.HasCheckConstraint("ck_reserva_quantidade", "quantity > 0"));
        builder.HasKey(Line => Line.Id);
        builder.Property(Line => Line.Id).ValueGeneratedOnAdd();
        builder.HasOne(Line => Line.StockReservation).WithMany(Reservation => Reservation.Lines)
            .HasForeignKey(Line => new { Line.EnterpriseId, Line.StockReservationId })
            .HasPrincipalKey(Reservation => new { Reservation.EnterpriseId, Reservation.Id }).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(Line => Line.ProductVariant).WithMany()
            .HasForeignKey(Line => new { Line.EnterpriseId, Line.ProductVariantId })
            .HasPrincipalKey(Variant => new { Variant.EnterpriseId, Variant.Id }).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(Line => new { Line.StockReservationId, Line.ProductVariantId }).IsUnique();
    }
}
