using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.ToTable("entrada_fila");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.SessionToken).IsRequired().HasMaxLength(200);
        builder.Property(e => e.DeviceFingerprint).IsRequired().HasMaxLength(500);
        builder.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(e => e.UserAgent).IsRequired().HasMaxLength(500);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListQueueEntry).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.QueueEntryStatus).WithMany().HasForeignKey(e => e.QueueEntryStatusId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListQueueSession).WithOne(e => e.QueueEntry).HasForeignKey(e => e.QueueEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropReservation).WithOne(e => e.QueueEntry).HasForeignKey(e => e.QueueEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListFraudSignal).WithOne(e => e.QueueEntry).HasForeignKey(e => e.QueueEntryId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.QueueEntryStatusId);
    }
}
