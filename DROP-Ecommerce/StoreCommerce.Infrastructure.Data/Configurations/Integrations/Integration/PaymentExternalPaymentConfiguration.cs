using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class PaymentExternalPaymentConfiguration : IEntityTypeConfiguration<PaymentExternalPayment>
{
    public void Configure(EntityTypeBuilder<PaymentExternalPayment> builder)
    {
        builder.ToTable("pagamento_externo");
        builder.HasKey(Payment => Payment.Id);
        builder.Property(Payment => Payment.Provider).HasMaxLength(40);
        builder.Property(Payment => Payment.ExternalAccountId).HasMaxLength(200);
        builder.Property(Payment => Payment.PaymentId).HasMaxLength(200);
        builder.HasOne(Payment => Payment.CapturedTransaction).WithMany()
            .HasForeignKey(Payment => Payment.CapturedTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(Payment => Payment.CapturedTransactionId).IsUnique();
        // A mesma cobrança não pode ser apropriada por outra loja que use a mesma conta externa.
        builder.HasIndex(Payment => new { Payment.Provider, Payment.Environment, Payment.ExternalAccountId, Payment.PaymentId }).IsUnique();
        builder.HasOne(Payment => Payment.PaymentAttempt).WithMany(Attempt => Attempt.Payments)
            .HasForeignKey(Payment => new
            {
                Payment.PaymentAttemptId, Payment.EnterpriseId, Payment.Provider, Payment.Environment, Payment.ExternalAccountId
            })
            .HasPrincipalKey(Attempt => new
            {
                Attempt.Id, Attempt.EnterpriseId, Attempt.Provider, Attempt.Environment, Attempt.ExternalAccountId
            }).OnDelete(DeleteBehavior.Restrict);
    }
}
