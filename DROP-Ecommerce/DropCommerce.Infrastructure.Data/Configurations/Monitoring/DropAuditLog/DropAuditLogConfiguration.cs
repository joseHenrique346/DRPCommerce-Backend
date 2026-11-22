using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropAuditLogConfiguration : IEntityTypeConfiguration<DropAuditLog>
{
    public void Configure(EntityTypeBuilder<DropAuditLog> builder)
    {
        builder.ToTable("log_auditoria");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Action).IsRequired().HasMaxLength(200);
        builder.Property(e => e.EntityName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.OldValues).HasMaxLength(4000);
        builder.Property(e => e.NewValues).HasMaxLength(4000);
        builder.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(e => e.UserAgent).IsRequired().HasMaxLength(500);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListDropAuditLog).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.EntityName);
    }
}
