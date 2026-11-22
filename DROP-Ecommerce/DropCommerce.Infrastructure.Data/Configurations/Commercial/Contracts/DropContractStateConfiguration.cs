using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public sealed class DropContractStateConfiguration : IEntityTypeConfiguration<DropContractState>
{
    public void Configure(EntityTypeBuilder<DropContractState> builder)
    {
        builder.ToTable("contrato_comercial");
        builder.HasKey(State => State.Id);
        builder.HasIndex(State => State.EnterpriseId).IsUnique();
    }
}
