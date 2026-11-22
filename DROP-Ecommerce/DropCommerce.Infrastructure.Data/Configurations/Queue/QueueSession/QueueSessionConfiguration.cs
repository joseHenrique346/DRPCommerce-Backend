using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class QueueSessionConfiguration : IEntityTypeConfiguration<QueueSession>
{
    public void Configure(EntityTypeBuilder<QueueSession> builder)
    {
        builder.ToTable("sessao_fila");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Token).IsRequired().HasMaxLength(200);

        builder.HasOne(e => e.QueueEntry).WithMany(e => e.ListQueueSession).HasForeignKey(e => e.QueueEntryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.QueueSessionStatus).WithMany().HasForeignKey(e => e.QueueSessionStatusId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.QueueEntryId);
        builder.HasIndex(e => e.CustomerId);
    }
}
