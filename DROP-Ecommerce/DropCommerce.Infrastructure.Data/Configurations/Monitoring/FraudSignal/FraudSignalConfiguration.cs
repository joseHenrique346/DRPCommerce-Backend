using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class FraudSignalConfiguration : IEntityTypeConfiguration<FraudSignal>
{
    public void Configure(EntityTypeBuilder<FraudSignal> builder)
    {
        builder.ToTable("sinal_fraude");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(e => e.DeviceFingerprint).IsRequired().HasMaxLength(500);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListFraudSignal).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.QueueEntry).WithMany(e => e.ListFraudSignal).HasForeignKey(e => e.QueueEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FraudSignalType).WithMany().HasForeignKey(e => e.FraudSignalTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FraudSeverity).WithMany().HasForeignKey(e => e.FraudSeverityId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.QueueEntryId);
    }
}
