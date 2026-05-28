using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EnterpriseConfiguration : IEntityTypeConfiguration<Enterprise>
{
    public void Configure(EntityTypeBuilder<Enterprise> builder)
    {
        builder.ToTable("empresa");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.TradeName).HasMaxLength(200);
        builder.Property(e => e.LegalName).HasMaxLength(200);
        builder.Property(e => e.AddressLine).HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.State).HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);

        builder.OwnsOne(e => e.Email, nav => { nav.Property(p => p.Value).HasColumnName("Email").HasMaxLength(200).IsRequired(); });
        builder.OwnsOne(e => e.Phone, nav => { nav.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(200).IsRequired(); });
    }
}
