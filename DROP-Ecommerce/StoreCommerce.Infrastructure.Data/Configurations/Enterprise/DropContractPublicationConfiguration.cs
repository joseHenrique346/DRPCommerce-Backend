using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class DropContractPublicationConfiguration : IEntityTypeConfiguration<DropContractPublication>
{
    public void Configure(EntityTypeBuilder<DropContractPublication> builder)
    {
        builder.ToTable("publicacao_contrato_drop");
        builder.HasKey(Publication => Publication.Id);
        builder.HasIndex(Publication => Publication.EnterpriseId).IsUnique();
        builder.HasOne(Publication => Publication.Enterprise).WithMany().HasForeignKey(Publication => Publication.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
