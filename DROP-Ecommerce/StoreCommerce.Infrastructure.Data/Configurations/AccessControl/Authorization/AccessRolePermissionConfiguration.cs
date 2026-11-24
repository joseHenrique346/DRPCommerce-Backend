using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class AccessRolePermissionConfiguration : IEntityTypeConfiguration<AccessRolePermission>
{
    public void Configure(EntityTypeBuilder<AccessRolePermission> builder)
    {
        builder.ToTable("papel_acesso_permissao");
        builder.HasKey(e => new { e.AccessRoleId, e.PermissionId });
        builder.HasOne(e => e.AccessRole).WithMany(e => e.ListAccessRolePermission)
            .HasForeignKey(e => new { e.EnterpriseId, e.AccessRoleId })
            .HasPrincipalKey(e => new { e.EnterpriseId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Permission).WithMany(e => e.ListAccessRolePermission)
            .HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Restrict);
    }
}
