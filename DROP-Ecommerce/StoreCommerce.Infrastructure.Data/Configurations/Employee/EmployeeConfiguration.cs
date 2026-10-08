using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("funcionario");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(255);
        builder.HasOne(e => e.Enterprise).WithMany(e => e.ListEmployee)
            .HasForeignKey(e => e.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EnterpriseMembership).WithOne(e => e.Employee)
            .HasForeignKey<Employee>(e => new { e.EnterpriseId, e.EnterpriseMembershipId })
            .HasPrincipalKey<EnterpriseMembership>(e => new { e.EnterpriseId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Position).WithMany(e => e.ListEmployee)
            .HasForeignKey(e => new { e.EnterpriseId, e.PositionId })
            .HasPrincipalKey(e => new { e.EnterpriseId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Department).WithMany(e => e.ListEmployee)
            .HasForeignKey(e => new { e.EnterpriseId, e.DepartmentId })
            .HasPrincipalKey(e => new { e.EnterpriseId, e.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.EnterpriseMembershipId).IsUnique();
    }
}
