using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropNotificationConfiguration : IEntityTypeConfiguration<DropNotification>
{
    public void Configure(EntityTypeBuilder<DropNotification> builder)
    {
        builder.ToTable("notificacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Subject).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListDropNotification).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropNotificationChannel).WithMany().HasForeignKey(e => e.DropNotificationChannelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropNotificationType).WithMany().HasForeignKey(e => e.DropNotificationTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropNotificationStatus).WithMany().HasForeignKey(e => e.DropNotificationStatusId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.DropNotificationStatusId);
    }
}
