using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> b)
    {
        b.ToTable("usuario_refresh_token");
        b.HasKey(x => x.Id);
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => new { x.UserId, x.FamilyId });
        b.HasIndex(x => x.FamilyExpiresAt);
        b.Property(x => x.ConsumedAt).IsConcurrencyToken();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class CustomerRefreshTokenConfiguration : IEntityTypeConfiguration<CustomerRefreshToken>
{
    public void Configure(EntityTypeBuilder<CustomerRefreshToken> b)
    {
        b.ToTable("cliente_refresh_token");
        b.HasKey(x => x.Id);
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => new { x.CustomerId, x.FamilyId });
        b.HasIndex(x => x.FamilyExpiresAt);
        b.Property(x => x.ConsumedAt).IsConcurrencyToken();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => new { x.EnterpriseId, x.CustomerId })
            .HasPrincipalKey(x => new { x.EnterpriseId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
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
public sealed class EnterpriseInvitationConfiguration : IEntityTypeConfiguration<EnterpriseInvitation>
{
    public void Configure(EntityTypeBuilder<EnterpriseInvitation> b)
    {
        b.ToTable("convite_empresa");
        b.HasKey(x => x.Id);
        b.HasAlternateKey(x => new { x.EnterpriseId, x.Id });
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.Property(x => x.InvitedNormalizedEmail).HasMaxLength(255).IsRequired();
        b.HasIndex(x => new { x.EnterpriseId, x.InvitedNormalizedEmail });
        b.HasIndex(x => x.ExpiresAt);
        b.Property(x => x.AcceptedAt).IsConcurrencyToken();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.HasOne(x => x.Enterprise).WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.IssuedByMembership).WithMany().HasForeignKey(x => new { x.EnterpriseId, x.IssuedByMembershipId })
            .HasPrincipalKey(x => new { x.EnterpriseId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class InvitationAccessRoleConfiguration : IEntityTypeConfiguration<InvitationAccessRole>
{
    public void Configure(EntityTypeBuilder<InvitationAccessRole> b)
    {
        b.ToTable("convite_empresa_papel_acesso");
        b.HasKey(x => new { x.EnterpriseInvitationId, x.AccessRoleId });
        b.HasOne(x => x.EnterpriseInvitation).WithMany(x => x.AccessRoles).HasForeignKey(x => new { x.EnterpriseId, x.EnterpriseInvitationId })
            .HasPrincipalKey(x => new { x.EnterpriseId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.AccessRole).WithMany().HasForeignKey(x => new { x.EnterpriseId, x.AccessRoleId })
            .HasPrincipalKey(x => new { x.EnterpriseId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class OwnershipTransferConfiguration : IEntityTypeConfiguration<OwnershipTransfer>
{
    public void Configure(EntityTypeBuilder<OwnershipTransfer> b)
    {
        b.ToTable("transferencia_propriedade");
        b.HasKey(x => x.Id);
        b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => new { x.EnterpriseId, x.ExpiresAt });
        b.Property(x => x.AcceptedAt).IsConcurrencyToken();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.HasOne(x => x.Enterprise).WithMany().HasForeignKey(x => x.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.IssuedByMembership).WithMany().HasForeignKey(x => new { x.EnterpriseId, x.IssuedByMembershipId })
            .HasPrincipalKey(x => new { x.EnterpriseId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RecipientMembership).WithMany().HasForeignKey(x => new { x.EnterpriseId, x.RecipientMembershipId })
            .HasPrincipalKey(x => new { x.EnterpriseId, x.Id }).OnDelete(DeleteBehavior.Restrict);
    }
}
