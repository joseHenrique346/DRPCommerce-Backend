using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.GatewayReference).HasMaxLength(200);
        builder.Property(e => e.GatewayProvider).HasMaxLength(200);
        builder.Property(e => e.GatewayPayload).HasMaxLength(4000);

        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Fee).HasPrecision(18, 2);

        builder.HasOne(e => e.Order)
            .WithMany(e => e.ListTransaction)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(e => e.ListTransaction)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TransactionType)
            .WithMany()
            .HasForeignKey(e => e.TransactionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TransactionMethod)
            .WithMany()
            .HasForeignKey(e => e.TransactionMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TransactionStatus)
            .WithMany()
            .HasForeignKey(e => e.TransactionStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.TransactionTypeId);
        builder.HasIndex(e => e.TransactionMethodId);
        builder.HasIndex(e => e.TransactionStatusId);
    }
}
