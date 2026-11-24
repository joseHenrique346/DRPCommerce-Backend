using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class UserActionTokenConfiguration : IEntityTypeConfiguration<UserActionToken>
{
    public void Configure(EntityTypeBuilder<UserActionToken> b)
    {
        b.ToTable("usuario_token_acao");
        b.HasKey(x => x.Id);
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => new { x.UserId, x.ExpiresAt });
        b.Property(x => x.ConsumedAt).IsConcurrencyToken();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
