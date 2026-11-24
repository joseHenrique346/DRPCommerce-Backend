using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropReservationConfiguration : IEntityTypeConfiguration<DropReservation>
{
    public void Configure(EntityTypeBuilder<DropReservation> builder)
    {
        builder.Property(Reservation => Reservation.PurchaseIdentityHash).HasMaxLength(64);
        builder.Property(Reservation => Reservation.IdempotencyKey).HasMaxLength(100);
        builder.HasIndex(Reservation => new { Reservation.QueueSessionId, Reservation.CustomerId, Reservation.IdempotencyKey }).IsUnique();
        builder.HasOne<QueueSession>().WithMany().HasForeignKey(Reservation => Reservation.QueueSessionId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable("reserva");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.LockToken).IsRequired().HasMaxLength(200);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListDropReservation).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropProduct).WithMany(e => e.ListDropReservation).HasForeignKey(e => e.DropProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.QueueEntry).WithMany(e => e.ListDropReservation).HasForeignKey(e => e.QueueEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropReservationStatus).WithMany().HasForeignKey(e => e.DropReservationStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropOrder).WithOne(e => e.DropReservation).HasForeignKey(e => e.DropReservationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.DropReservationStatusId);
    }
}
