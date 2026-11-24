using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

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
