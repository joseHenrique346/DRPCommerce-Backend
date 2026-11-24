using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ToTable("entrada_lista_espera");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListWaitlistEntry).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropProduct).WithMany(e => e.ListWaitlistEntry).HasForeignKey(e => e.DropProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.WaitlistEntryStatus).WithMany().HasForeignKey(e => e.WaitlistEntryStatusId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.WaitlistEntryStatusId);
    }
}
