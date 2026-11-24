using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class ProviderAuthorizationConfiguration : IEntityTypeConfiguration<ProviderAuthorization>
{
    public void Configure(EntityTypeBuilder<ProviderAuthorization> builder)
    {
        builder.ToTable("autorizacao_provedor");
        builder.HasKey(Authorization => Authorization.Id);
        builder.Property(Authorization => Authorization.StateHash).HasMaxLength(64);
        builder.Property(Authorization => Authorization.BrowserHash).HasMaxLength(64);
        builder.Property(Authorization => Authorization.ProtectedVerifier).HasMaxLength(4000);
        builder.Property(Authorization => Authorization.Provider).HasMaxLength(40);
        builder.HasIndex(Authorization => Authorization.StateHash).IsUnique();
        builder.HasIndex(Authorization => Authorization.ExpiresAt);
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Authorization => Authorization.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(Authorization => Authorization.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
