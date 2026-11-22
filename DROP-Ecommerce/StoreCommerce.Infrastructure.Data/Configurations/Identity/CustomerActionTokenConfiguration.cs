using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class CustomerActionTokenConfiguration : IEntityTypeConfiguration<CustomerActionToken>
{
    public void Configure(EntityTypeBuilder<CustomerActionToken> b)
    {
        b.ToTable("cliente_token_acao");
        b.HasKey(x => x.Id);
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => new { x.CustomerId, x.ExpiresAt });
        b.Property(x => x.ConsumedAt).IsConcurrencyToken();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => new { x.EnterpriseId, x.CustomerId })
            .HasPrincipalKey(x => new { x.EnterpriseId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
