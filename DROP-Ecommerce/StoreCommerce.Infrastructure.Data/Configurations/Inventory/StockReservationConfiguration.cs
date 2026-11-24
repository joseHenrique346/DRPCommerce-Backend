using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("reserva_estoque");
        builder.HasKey(Reservation => Reservation.Id);
        builder.Property(Reservation => Reservation.Id).ValueGeneratedOnAdd();
        builder.HasAlternateKey(Reservation => new { Reservation.EnterpriseId, Reservation.Id });
        builder.Property(Reservation => Reservation.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.Property(Reservation => Reservation.Reference).HasMaxLength(200);
        builder.Property(Reservation => Reservation.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(Reservation => Reservation.Version).IsConcurrencyToken();
        builder.HasIndex(Reservation => new { Reservation.EnterpriseId, Reservation.IdempotencyKey }).IsUnique();
        builder.HasIndex(Reservation => new { Reservation.Status, Reservation.ExpiresAt, Reservation.EnterpriseId });
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Reservation => Reservation.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(Reservation => Reservation.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
