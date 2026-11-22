using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class PaymentOperationConfiguration : IEntityTypeConfiguration<PaymentOperation>
{
    public void Configure(EntityTypeBuilder<PaymentOperation> builder)
    {
        builder.ToTable("operacao_pagamento");
        builder.HasKey(Operation => Operation.Id);
        builder.Property(Operation => Operation.Provider).HasMaxLength(200);
        builder.Property(Operation => Operation.ExternalAccountId).HasMaxLength(200);
        builder.Property(Operation => Operation.ExternalReference).HasMaxLength(64);
        builder.Property(Operation => Operation.IdempotencyKey).HasMaxLength(64);
        builder.Property(Operation => Operation.PaymentId).HasMaxLength(200);
        builder.Property(Operation => Operation.PreferenceId).HasMaxLength(200);
        builder.Property(Operation => Operation.CurrencyCode).HasMaxLength(3);
        builder.Property(Operation => Operation.Reason).HasMaxLength(1000);
        builder.Property(Operation => Operation.ResultReference).HasMaxLength(200);
        builder.Property(Operation => Operation.ResolutionCode).HasMaxLength(64);
        builder.Property(Operation => Operation.ManualConfirmationNote).HasMaxLength(1000);
        builder.Property(Operation => Operation.Amount).HasPrecision(18, 2);
        builder.Property(Operation => Operation.Version).IsConcurrencyToken();
        builder.HasIndex(Operation => Operation.IdempotencyKey).IsUnique();
        builder.HasIndex(Operation => new { Operation.PaymentTransactionId, Operation.Kind }).IsUnique();
        builder.HasIndex(Operation => new { Operation.EnterpriseId, Operation.OrderId, Operation.Kind })
            .IsUnique().HasFilter("payment_transaction_id IS NULL");
        builder.HasIndex(Operation => new { Operation.Status, Operation.NextReconciliationAt });
        builder.HasOne(Operation => Operation.Order).WithMany(Order => Order.ListPaymentOperation)
            .HasForeignKey(Operation => Operation.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Transaction>().WithMany().HasForeignKey(Operation => Operation.PaymentTransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PaymentAttempt>().WithMany().HasForeignKey(Operation => Operation.PaymentAttemptId).OnDelete(DeleteBehavior.Restrict);
    }
}
