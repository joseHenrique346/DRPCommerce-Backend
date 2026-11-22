using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class PaymentProviderSelectionConfiguration : IEntityTypeConfiguration<PaymentProviderSelection>
{
    public void Configure(EntityTypeBuilder<PaymentProviderSelection> builder)
    {
        builder.ToTable("selecao_provedor_pagamento");
        builder.HasKey(Selection => Selection.Id);
        builder.Property(Selection => Selection.Provider).HasMaxLength(40);
        builder.HasIndex(Selection => Selection.EnterpriseId).IsUnique();
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Selection => Selection.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
