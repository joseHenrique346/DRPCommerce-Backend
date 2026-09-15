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

        builder.Property(e => e.FullName).HasMaxLength(200);
        builder.Property(e => e.PasswordHash).HasMaxLength(500);

        builder.OwnsOne(e => e.Email, nav =>
        {
            nav.Property(p => p.Value).HasColumnName("Email").HasMaxLength(200).IsRequired();
        });

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListEmployee)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Role)
            .WithMany(e => e.ListEmployee)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(e => e.ListEmployee)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.RoleId);
        builder.HasIndex(e => e.DepartmentId);
    }
}
