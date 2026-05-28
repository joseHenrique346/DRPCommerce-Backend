using DropCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropNotificationChannelConfiguration : IEntityTypeConfiguration<DropNotificationChannel>
{
    public void Configure(EntityTypeBuilder<DropNotificationChannel> builder)
    {
        builder.ToTable("canal_notificacao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(100);

        builder.HasData(
            new { Id = 1L, Description = "E-mail" },
            new { Id = 2L, Description = "SMS" },
            new { Id = 3L, Description = "Push" },
            new { Id = 4L, Description = "WhatsApp" }
        );
    }
}
