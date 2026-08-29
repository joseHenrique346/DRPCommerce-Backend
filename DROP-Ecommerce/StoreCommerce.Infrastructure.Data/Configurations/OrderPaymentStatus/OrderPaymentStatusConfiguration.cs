using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class OrderPaymentStatusConfiguration : IEntityTypeConfiguration<OrderPaymentStatus>
{
    public void Configure(EntityTypeBuilder<OrderPaymentStatus> builder)
    {
        builder.ToTable("status_pagamento_pedido");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pendente" },
            new { Id = 2L, Description = "Pago" },
            new { Id = 3L, Description = "Reembolso parcial" },
            new { Id = 4L, Description = "Reembolso total" },
            new { Id = 5L, Description = "Falhou" }
        );
    }
}
