using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class StoreSettingsConfiguration : IEntityTypeConfiguration<StoreSettings>
{
    public void Configure(EntityTypeBuilder<StoreSettings> builder)
    {
        builder.ToTable("configuracao_loja");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(StoreSettings.CurrencyCodeLength).IsFixedLength();
        builder.Property(e => e.MinOrderValue).HasPrecision(18, 2);

        // Uma configuração por loja.
        builder.HasOne(e => e.Enterprise)
            .WithOne()
            .HasForeignKey<StoreSettings>(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId).IsUnique();
    }
}
