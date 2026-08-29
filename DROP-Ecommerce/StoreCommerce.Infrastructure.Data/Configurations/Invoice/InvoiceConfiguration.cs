using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("nota_fiscal");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Number).HasMaxLength(100);
        builder.Property(e => e.Series).HasMaxLength(100);
        builder.Property(e => e.AccessKey).HasMaxLength(500);
        builder.Property(e => e.FileUrl).HasMaxLength(500);

        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListInvoice)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Order)
            .WithMany(e => e.ListInvoice)
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(e => e.ListInvoice)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InvoiceType)
            .WithMany()
            .HasForeignKey(e => e.InvoiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InvoiceStatus)
            .WithMany()
            .HasForeignKey(e => e.InvoiceStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.InvoiceTypeId);
        builder.HasIndex(e => e.InvoiceStatusId);
        builder.HasIndex(e => e.Number);
    }
}
