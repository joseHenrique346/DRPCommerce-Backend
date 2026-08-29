using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documento");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.ReferenceType).HasMaxLength(500);
        builder.Property(e => e.Number).HasMaxLength(100);
        builder.Property(e => e.FileUrl).HasMaxLength(500);

        builder.HasOne(e => e.Enterprise)
            .WithMany(e => e.ListDocument)
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DocumentType)
            .WithMany()
            .HasForeignKey(e => e.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DocumentStatus)
            .WithMany()
            .HasForeignKey(e => e.DocumentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EnterpriseId);
        builder.HasIndex(e => e.DocumentTypeId);
        builder.HasIndex(e => e.DocumentStatusId);
        builder.HasIndex(e => new { e.ReferenceId, e.ReferenceType });
    }
}
