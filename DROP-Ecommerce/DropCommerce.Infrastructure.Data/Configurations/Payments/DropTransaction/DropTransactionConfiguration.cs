using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropTransactionConfiguration : IEntityTypeConfiguration<DropTransaction>
{
    public void Configure(EntityTypeBuilder<DropTransaction> builder)
    {
        builder.ToTable("transacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.Fee).HasPrecision(18, 2);
        builder.Property(e => e.GatewayReference).IsRequired().HasMaxLength(200);
        builder.Property(e => e.GatewayProvider).IsRequired().HasMaxLength(100);
        builder.Property(e => e.GatewayPayload).IsRequired().HasMaxLength(4000);

        builder.HasOne(e => e.DropOrder).WithMany(e => e.ListDropTransaction).HasForeignKey(e => e.DropOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropTransactionType).WithMany().HasForeignKey(e => e.DropTransactionTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropTransactionMethod).WithMany().HasForeignKey(e => e.DropTransactionMethodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropTransactionStatus).WithMany().HasForeignKey(e => e.DropTransactionStatusId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropOrderId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.DropTransactionStatusId);
    }
}
