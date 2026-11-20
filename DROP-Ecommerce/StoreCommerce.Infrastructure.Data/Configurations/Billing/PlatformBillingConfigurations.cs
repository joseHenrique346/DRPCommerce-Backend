using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class PlatformSubscriptionConfiguration : IEntityTypeConfiguration<PlatformSubscription>
{
    public void Configure(EntityTypeBuilder<PlatformSubscription> builder)
    {
        builder.ToTable("assinatura_plataforma");
        builder.HasKey(Subscription => Subscription.Id);
        builder.Property(Subscription => Subscription.Reference).HasMaxLength(41);
        builder.Property(Subscription => Subscription.AccountId).HasMaxLength(40);
        builder.Property(Subscription => Subscription.RemoteId).HasMaxLength(200);
        builder.Property(Subscription => Subscription.CheckoutUrl).HasMaxLength(2000);
        builder.Property(Subscription => Subscription.RemoteStatus).HasMaxLength(40);
        builder.Property(Subscription => Subscription.ReviewCode).HasMaxLength(100);
        builder.Property(Subscription => Subscription.RetainedMembershipIdsJson).HasColumnType("jsonb");
        builder.HasIndex(Subscription => Subscription.Reference).IsUnique();
        builder.HasIndex(Subscription => new { Subscription.AccountId, Subscription.Environment, Subscription.RemoteId }).IsUnique();
        builder.HasIndex(Subscription => Subscription.EnterpriseId).IsUnique().HasFilter("closed_at IS NULL");
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Subscription => Subscription.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterpriseContractChange>().WithMany().HasForeignKey(Subscription => Subscription.OfferId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterpriseContractChange>().WithMany().HasForeignKey(Subscription => Subscription.PendingOfferId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PlatformBillingInvoiceConfiguration : IEntityTypeConfiguration<PlatformBillingInvoice>
{
    public void Configure(EntityTypeBuilder<PlatformBillingInvoice> builder)
    {
        builder.ToTable("fatura_plataforma");
        builder.HasKey(Invoice => Invoice.Id);
        builder.Property(Invoice => Invoice.Origin).HasMaxLength(220);
        builder.Property(Invoice => Invoice.Reference).HasMaxLength(41);
        builder.Property(Invoice => Invoice.Amount).HasPrecision(12, 2);
        builder.Property(Invoice => Invoice.MonthlyAmount).HasPrecision(12, 2);
        builder.Property(Invoice => Invoice.Currency).HasMaxLength(3);
        builder.Property(Invoice => Invoice.PreferenceId).HasMaxLength(200);
        builder.Property(Invoice => Invoice.CheckoutUrl).HasMaxLength(2000);
        builder.Property(Invoice => Invoice.PaymentId).HasMaxLength(200);
        builder.Property(Invoice => Invoice.Status).HasMaxLength(40);
        builder.Property(Invoice => Invoice.ReviewCode).HasMaxLength(100);
        builder.HasIndex(Invoice => Invoice.Reference).IsUnique();
        builder.HasIndex(Invoice => new { Invoice.SubscriptionId, Invoice.Origin }).IsUnique();
        builder.HasIndex(Invoice => Invoice.PaymentId).IsUnique();
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Invoice => Invoice.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlatformSubscription>().WithMany().HasForeignKey(Invoice => Invoice.SubscriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterpriseContractChange>().WithMany().HasForeignKey(Invoice => Invoice.OfferId).OnDelete(DeleteBehavior.Restrict);
    }
}

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
