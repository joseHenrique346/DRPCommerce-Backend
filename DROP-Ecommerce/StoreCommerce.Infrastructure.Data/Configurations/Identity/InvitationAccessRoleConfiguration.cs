using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

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
