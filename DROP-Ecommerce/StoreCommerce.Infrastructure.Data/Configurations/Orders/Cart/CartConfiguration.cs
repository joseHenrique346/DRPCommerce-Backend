using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carrinho");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(e => e.EnterpriseId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ListCartItem).WithOne(e => e.Cart).HasForeignKey(e => e.CartId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(e => e.ListCartItem).HasField("_listCartItem");

        // Um carrinho por cliente.
        builder.HasIndex(e => new { e.EnterpriseId, e.CustomerId }).IsUnique();
    }
}
