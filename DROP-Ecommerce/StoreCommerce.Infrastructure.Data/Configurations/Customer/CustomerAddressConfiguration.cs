using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("endereco_cliente");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Label).HasMaxLength(CustomerAddress.LabelMaxLength);
        builder.Property(e => e.RecipientName).HasMaxLength(CustomerAddress.RecipientNameMaxLength);
        builder.Property(e => e.AddressLine).IsRequired().HasMaxLength(CustomerAddress.AddressLineMaxLength);
        builder.Property(e => e.City).IsRequired().HasMaxLength(CustomerAddress.CityMaxLength);
        builder.Property(e => e.ZipCode).IsRequired().HasMaxLength(CustomerAddress.ZipCodeMaxLength);
        builder.Property(e => e.Country).IsRequired().HasMaxLength(CustomerAddress.CountryMaxLength);

        builder.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.State).WithMany().HasForeignKey(e => e.StateId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.StateId);
        // O padrão único por cliente é garantido pela aplicação, com o cliente bloqueado durante a alteração. Um índice único
        // parcial não serve aqui: trocar o padrão gera dois UPDATEs e o PostgreSQL validaria o índice entre eles.
        builder.HasIndex(e => new { e.CustomerId, e.IsDefault });
    }
}
