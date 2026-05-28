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

        builder.HasIndex(e => e.EnterpriseId);

        builder.HasOne<Enterprise>()
            .WithMany()
            .HasForeignKey(e => e.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
