using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EmployeePositionConfiguration : IEntityTypeConfiguration<EmployeePosition>
{
    public void Configure(EntityTypeBuilder<EmployeePosition> builder)
    {
        builder.ToTable("cargo");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(EmployeePosition.NameMaxLength);
        builder.Property(e => e.NormalizedName).IsRequired().HasMaxLength(EmployeePosition.NormalizedNameMaxLength);
        builder.Property(e => e.Description).HasMaxLength(EmployeePosition.DescriptionMaxLength);
        builder.HasAlternateKey(e => new { e.EnterpriseId, e.Id });
        builder.HasIndex(e => new { e.EnterpriseId, e.NormalizedName }).IsUnique();
        builder.HasOne(e => e.Enterprise).WithMany(e => e.ListEmployeePosition)
            .HasForeignKey(e => e.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
