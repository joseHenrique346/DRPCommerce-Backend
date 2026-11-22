using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class PlatformBillingNotificationConfiguration : IEntityTypeConfiguration<PlatformBillingNotification>
{
    public void Configure(EntityTypeBuilder<PlatformBillingNotification> builder)
    {
        builder.ToTable("notificacao_cobranca_plataforma");
        builder.HasKey(Notification => Notification.Id);
        builder.Property(Notification => Notification.Delivery).HasMaxLength(64);
        builder.Property(Notification => Notification.Topic).HasMaxLength(50);
        builder.Property(Notification => Notification.ResourceId).HasMaxLength(200);
        builder.Property(Notification => Notification.AccountId).HasMaxLength(40);
        builder.HasIndex(Notification => new { Notification.AccountId, Notification.Environment, Notification.Topic, Notification.Delivery }).IsUnique();
        builder.HasIndex(Notification => new { Notification.CompletedAt, Notification.NextCheckAt });
    }
}
