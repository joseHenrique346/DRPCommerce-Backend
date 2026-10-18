using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EnterpriseMembershipConfiguration : IEntityTypeConfiguration<EnterpriseMembership>
{
    public void Configure(EntityTypeBuilder<EnterpriseMembership> builder)
    {
        builder.ToTable("membro_empresa");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.AccessVersion).HasDefaultValue(1L);
        builder.HasAlternateKey(e => new { e.EnterpriseId, e.Id });
        builder.HasIndex(e => new { e.EnterpriseId, e.UserId }).IsUnique();
        builder.HasIndex(e => e.EnterpriseId).IsUnique()
            .HasFilter("kind_id = 1 AND is_active = TRUE")
            .HasDatabaseName("ux_membro_empresa_dono_principal_ativo");
        builder.HasOne(e => e.Enterprise).WithMany(e => e.ListEnterpriseMembership)
            .HasForeignKey(e => e.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.User).WithMany(e => e.ListEnterpriseMembership)
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Kind).WithMany()
            .HasForeignKey(e => e.KindId).OnDelete(DeleteBehavior.Restrict);
    }
}
