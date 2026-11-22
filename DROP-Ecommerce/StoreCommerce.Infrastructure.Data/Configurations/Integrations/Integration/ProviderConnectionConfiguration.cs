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
