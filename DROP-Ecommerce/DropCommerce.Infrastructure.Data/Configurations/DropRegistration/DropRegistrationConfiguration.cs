using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public class DropRegistrationConfiguration : IEntityTypeConfiguration<DropRegistration>
{
    public void Configure(EntityTypeBuilder<DropRegistration> builder)
    {
        builder.ToTable("inscricao");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.EligibilityReason).IsRequired().HasMaxLength(500);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.HasOne(e => e.DropEvent).WithMany(e => e.ListDropRegistration).HasForeignKey(e => e.DropEventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DropRegistrationStatus).WithMany().HasForeignKey(e => e.DropRegistrationStatusId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DropEventId);
        builder.HasIndex(e => e.CustomerId);
    }
}
