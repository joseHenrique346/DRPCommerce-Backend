using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("usuario");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.NormalizedEmail).IsRequired().HasMaxLength(255);
        builder.Property(e => e.PasswordHash).HasMaxLength(500);
        builder.Property(e => e.SecurityVersion).HasDefaultValue(1L);
        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property<long>("UserId").HasColumnName("id");
            email.Property(e => e.Value).HasColumnName("email").IsRequired().HasMaxLength(255);
        });
        builder.HasIndex(e => e.NormalizedEmail).IsUnique();
    }
}
