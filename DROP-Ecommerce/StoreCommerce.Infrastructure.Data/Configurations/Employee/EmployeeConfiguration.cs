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

        builder.HasIndex(e => e.EnterpriseId);

        builder.HasOne<Enterprise>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RoleId)
            .WithMany()
            .HasForeignKey("RoleId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DepartmentId)
            .WithMany()
            .HasForeignKey("DepartmentId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
