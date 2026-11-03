using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("envio");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.CarrierName).HasMaxLength(Shipment.CarrierNameMaxLength);
        builder.Property(e => e.TrackingCode).HasMaxLength(Shipment.TrackingCodeMaxLength);
        builder.Property(e => e.AddressLine).HasMaxLength(Shipment.AddressLineMaxLength);
        builder.Property(e => e.City).HasMaxLength(Shipment.CityMaxLength);
        builder.Property(e => e.ZipCode).HasMaxLength(Shipment.ZipCodeMaxLength);
        builder.Property(e => e.Country).HasMaxLength(Shipment.CountryMaxLength);

        builder.Property(e => e.ShippingCost).HasPrecision(18, 2);

        builder.HasOne(e => e.Order)
            .WithMany(e => e.ListShipment)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Supplier)
            .WithMany(e => e.ListShipment)
            .HasForeignKey(e => e.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ShipmentType)
            .WithMany()
            .HasForeignKey(e => e.ShipmentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ShipmentStatus)
            .WithMany()
            .HasForeignKey(e => e.ShipmentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.State)
            .WithMany()
            .HasForeignKey(e => e.StateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.SupplierId);
        builder.HasIndex(e => e.ShipmentTypeId);
        builder.HasIndex(e => e.ShipmentStatusId);
        builder.HasIndex(e => e.StateId);
    }
}
