using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class DropSalePaymentIntentConfiguration : IEntityTypeConfiguration<DropSalePaymentIntent>
{
    public void Configure(EntityTypeBuilder<DropSalePaymentIntent> builder)
    {
        builder.ToTable("pagamento_venda_drop");
        builder.HasKey(Payment => Payment.Id);
        builder.Property(Payment => Payment.Amount).HasPrecision(18, 2);
        builder.Property(Payment => Payment.Currency).HasMaxLength(3);
        builder.Property(Payment => Payment.Provider).HasMaxLength(40);
        builder.Property(Payment => Payment.ExternalAccountId).HasMaxLength(200);
        builder.Property(Payment => Payment.ExternalReference).HasMaxLength(80);
        builder.Property(Payment => Payment.PreferenceId).HasMaxLength(200);
        builder.Property(Payment => Payment.PaymentId).HasMaxLength(200);
        builder.Property(Payment => Payment.CheckoutUrl).HasMaxLength(2000);
        builder.Property(Payment => Payment.Status).HasMaxLength(40);
        builder.HasIndex(Payment => new { Payment.EnterpriseId, Payment.OperationId }).IsUnique();
        builder.HasIndex(Payment => new { Payment.EnterpriseId, Payment.DropOrderId }).IsUnique();
        builder.HasIndex(Payment => Payment.ExternalReference).IsUnique();
    }
}
