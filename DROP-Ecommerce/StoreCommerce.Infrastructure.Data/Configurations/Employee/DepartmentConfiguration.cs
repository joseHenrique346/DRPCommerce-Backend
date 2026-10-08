using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departamento");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.NormalizedName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.HasAlternateKey(e => new { e.EnterpriseId, e.Id });
        builder.HasIndex(e => new { e.EnterpriseId, e.NormalizedName }).IsUnique();
        builder.HasOne(e => e.Enterprise).WithMany(e => e.ListDepartment)
            .HasForeignKey(e => e.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
