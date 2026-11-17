using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class ProviderConnectionConfiguration : IEntityTypeConfiguration<ProviderConnection>
{
    public void Configure(EntityTypeBuilder<ProviderConnection> builder)
    {
        builder.ToTable("conexao_provedor");
        builder.HasKey(Connection => Connection.Id);
        builder.Property(Connection => Connection.Provider).HasMaxLength(40);
        builder.Property(Connection => Connection.ExternalAccountId).HasMaxLength(200);
        builder.Property(Connection => Connection.Scopes).HasMaxLength(2000);
        builder.Property(Connection => Connection.ProtectedTokens).HasMaxLength(64000);
        builder.Property(Connection => Connection.Version).IsConcurrencyToken();
        builder.HasIndex(Connection => new { Connection.EnterpriseId, Connection.Provider, Connection.Environment }).IsUnique();
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Connection => Connection.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(Connection => Connection.AuthorizedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasAlternateKey(Connection => new
        {
            Connection.Id, Connection.EnterpriseId, Connection.Provider, Connection.Environment, Connection.ExternalAccountId
        });
    }
}

public sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("tentativa_pagamento");
        builder.HasKey(Attempt => Attempt.Id);
        builder.Property(Attempt => Attempt.Provider).HasMaxLength(40);
        builder.Property(Attempt => Attempt.ExternalAccountId).HasMaxLength(200);
        builder.Property(Attempt => Attempt.ExternalReference).HasMaxLength(64);
        builder.Property(Attempt => Attempt.IdempotencyKey).HasMaxLength(64);
        builder.Property(Attempt => Attempt.CurrencyCode).HasMaxLength(3);
        builder.Property(Attempt => Attempt.CustomerName).HasMaxLength(200);
        builder.Property(Attempt => Attempt.CustomerEmail).HasMaxLength(255);
        builder.Property(Attempt => Attempt.Amount).HasPrecision(18, 2);
        builder.Property(Attempt => Attempt.PreferenceId).HasMaxLength(200);
        builder.Property(Attempt => Attempt.CheckoutUrl).HasMaxLength(2000);
        builder.Property(Attempt => Attempt.Instructions).HasMaxLength(1000);
        builder.Property(Attempt => Attempt.Version).IsConcurrencyToken();
        builder.HasIndex(Attempt => Attempt.TransactionId).IsUnique();
        builder.HasIndex(Attempt => Attempt.ExternalReference).IsUnique();
        builder.HasIndex(Attempt => Attempt.IdempotencyKey).IsUnique();
        builder.HasIndex(Attempt => new { Attempt.EnterpriseId, Attempt.OrderId });
        builder.HasIndex(Attempt => new { Attempt.Status, Attempt.NextReconciliationAt });
        builder.HasIndex(Attempt => new { Attempt.Provider, Attempt.Environment, Attempt.ExternalAccountId, Attempt.PreferenceId })
            .IsUnique().HasFilter("preference_id IS NOT NULL");
        builder.HasOne(Attempt => Attempt.Order).WithMany(Order => Order.ListPaymentAttempt)
            .HasForeignKey(Attempt => Attempt.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(Attempt => Attempt.Transaction).WithMany().HasForeignKey(Attempt => Attempt.TransactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(Attempt => Attempt.ProviderConnection).WithMany()
            .HasForeignKey(Attempt => new
            {
                Attempt.ProviderConnectionId, Attempt.EnterpriseId, Attempt.Provider, Attempt.Environment, Attempt.ExternalAccountId
            })
            .HasPrincipalKey(Connection => new
            {
                Connection.Id, Connection.EnterpriseId, Connection.Provider, Connection.Environment, Connection.ExternalAccountId
            }).OnDelete(DeleteBehavior.Restrict);
        builder.HasAlternateKey(Attempt => new
        {
            Attempt.Id, Attempt.EnterpriseId, Attempt.Provider, Attempt.Environment, Attempt.ExternalAccountId
        });
    }
}

public sealed class PaymentExternalPaymentConfiguration : IEntityTypeConfiguration<PaymentExternalPayment>
{
    public void Configure(EntityTypeBuilder<PaymentExternalPayment> builder)
    {
        builder.ToTable("pagamento_externo");
        builder.HasKey(Payment => Payment.Id);
        builder.Property(Payment => Payment.Provider).HasMaxLength(40);
        builder.Property(Payment => Payment.ExternalAccountId).HasMaxLength(200);
        builder.Property(Payment => Payment.PaymentId).HasMaxLength(200);
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
