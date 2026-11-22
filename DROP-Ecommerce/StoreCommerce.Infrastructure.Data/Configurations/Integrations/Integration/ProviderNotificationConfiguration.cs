using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class ProviderNotificationConfiguration : IEntityTypeConfiguration<ProviderNotification>
{
    public void Configure(EntityTypeBuilder<ProviderNotification> builder)
    {
        builder.ToTable("notificacao_provedor");
        builder.HasKey(Notification => Notification.Id);
        builder.Property(Notification => Notification.PaymentId).HasMaxLength(200);
        builder.Property(Notification => Notification.DeliveryHash).HasMaxLength(64);
        builder.Property(Notification => Notification.ResolutionCode).HasMaxLength(80);
        builder.Property(Notification => Notification.Version).IsConcurrencyToken();
        builder.HasIndex(Notification => new { Notification.ProviderConnectionId, Notification.DeliveryHash }).IsUnique();
        builder.HasIndex(Notification => new { Notification.Status, Notification.NextAttemptAt });
        builder.HasOne(Notification => Notification.Connection).WithMany()
            .HasForeignKey(Notification => Notification.ProviderConnectionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PaymentAttempt>().WithMany()
            .HasForeignKey(Notification => Notification.PaymentAttemptId).OnDelete(DeleteBehavior.Restrict);
    }
}
