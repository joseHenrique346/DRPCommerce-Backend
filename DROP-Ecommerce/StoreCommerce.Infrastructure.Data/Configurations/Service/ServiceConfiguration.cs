using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("servico");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Name).HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.Property(e => e.Price).HasPrecision(18, 2);

        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(e => e.EnterpriseId);

        builder.HasOne<Enterprise>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StoreCommerce.Domain.Entity.Category.Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
