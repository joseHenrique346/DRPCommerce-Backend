using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public sealed class DropReservationLineConfiguration : IEntityTypeConfiguration<DropReservationLine>
{
    public void Configure(EntityTypeBuilder<DropReservationLine> builder)
    {
        builder.ToTable("linha_reserva_compra");
        builder.HasKey(Line => Line.Id);
        builder.Property(Line => Line.UnitPrice).HasPrecision(18, 2);
        builder.Property(Line => Line.ItemName).HasMaxLength(255);
        builder.Property(Line => Line.SKU).HasMaxLength(100);
        builder.Property(Line => Line.OptionsJson).HasColumnType("jsonb");
        builder.HasOne(Line => Line.Reservation).WithMany(Reservation => Reservation.Lines).HasForeignKey(Line => Line.ReservationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(Line => Line.Variant).WithMany().HasForeignKey(Line => Line.VariantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(Line => new { Line.ReservationId, Line.VariantId }).IsUnique();
    }
}
