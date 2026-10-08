using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class MembershipAccessRoleConfiguration : IEntityTypeConfiguration<MembershipAccessRole>
{
    public void Configure(EntityTypeBuilder<MembershipAccessRole> builder)
    {
        builder.ToTable("membro_empresa_papel_acesso");
        builder.HasKey(e => new { e.EnterpriseMembershipId, e.AccessRoleId });
        builder.HasOne(e => e.EnterpriseMembership).WithMany(e => e.ListMembershipAccessRole)
            .HasForeignKey(e => new { e.EnterpriseId, e.EnterpriseMembershipId })
            .HasPrincipalKey(e => new { e.EnterpriseId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.AccessRole).WithMany(e => e.ListMembershipAccessRole)
            .HasForeignKey(e => new { e.EnterpriseId, e.AccessRoleId })
            .HasPrincipalKey(e => new { e.EnterpriseId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
