using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropEventConfiguration : IEntityTypeConfiguration<DropEvent>
{
    public void Configure(EntityTypeBuilder<DropEvent> builder)
    {
        builder.ToTable("evento");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.CoverImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.BannerImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.DropEventStatus).WithMany().HasForeignKey(e => e.DropEventStatusId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListDropProduct).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropOrder).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropCoupon).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropRegistration).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropReservation).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListQueueEntry).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListWaitlistEntry).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropNotification).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListDropAuditLog).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.ListFraudSignal).WithOne(e => e.DropEvent).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Slug);
        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.DropEventStatusId);
    }
}
