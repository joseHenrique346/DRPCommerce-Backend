using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

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
