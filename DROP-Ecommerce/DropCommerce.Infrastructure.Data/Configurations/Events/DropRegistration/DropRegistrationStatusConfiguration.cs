using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropRegistrationStatusConfiguration : IEntityTypeConfiguration<DropRegistrationStatus>
{
    public void Configure(EntityTypeBuilder<DropRegistrationStatus> builder)
    {
        builder.ToTable("status_inscricao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "Pendente" },
            new { Id = 2L, Description = "Elegível" },
            new { Id = 3L, Description = "Inelegível" },
            new { Id = 4L, Description = "Na lista de espera" }
        );
    }
}
