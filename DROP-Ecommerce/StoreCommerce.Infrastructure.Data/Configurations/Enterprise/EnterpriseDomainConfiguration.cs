using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EnterpriseDomainConfiguration : IEntityTypeConfiguration<EnterpriseDomain>
{
    public void Configure(EntityTypeBuilder<EnterpriseDomain> builder)
    {
        builder.ToTable("dominio_empresa");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Host).IsRequired().HasMaxLength(253);
        builder.Property(e => e.NormalizedHost).IsRequired().HasMaxLength(253);
        builder.HasIndex(e => e.NormalizedHost).IsUnique();
        builder.HasIndex(e => e.EnterpriseId).IsUnique()
            .HasFilter("is_primary = TRUE").HasDatabaseName("ux_dominio_empresa_primario");
        builder.HasOne(e => e.Enterprise).WithMany(e => e.ListEnterpriseDomain)
            .HasForeignKey(e => e.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
