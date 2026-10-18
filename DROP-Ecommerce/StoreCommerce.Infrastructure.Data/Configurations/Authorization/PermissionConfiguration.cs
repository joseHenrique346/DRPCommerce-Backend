using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissao");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Module).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasData(
            new { Id = 1L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.ProductsView, Name = "Consultar produtos", Description = "Consultar produtos", Module = "Products", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 2L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.ProductsCreate, Name = "Cadastrar produtos", Description = "Cadastrar produtos", Module = "Products", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 3L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.ProductsEdit, Name = "Editar produtos", Description = "Editar produtos", Module = "Products", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 4L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.ProductsChangePrice, Name = "Alterar precos", Description = "Alterar precos", Module = "Products", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 5L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.ProductsDelete, Name = "Excluir produtos", Description = "Excluir produtos", Module = "Products", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 6L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.OrdersView, Name = "Consultar pedidos", Description = "Consultar pedidos", Module = "Orders", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 7L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.OrdersManage, Name = "Gerenciar pedidos", Description = "Gerenciar pedidos", Module = "Orders", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 8L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.CustomersView, Name = "Consultar clientes", Description = "Consultar clientes", Module = "Customers", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 9L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.CustomersManage, Name = "Gerenciar clientes", Description = "Gerenciar clientes", Module = "Customers", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 10L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.EmployeesManage, Name = "Gerenciar perfis funcionais", Description = "Gerenciar perfis funcionais", Module = "Employees", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 11L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.DropEventsView, Name = "Consultar drops", Description = "Consultar drops", Module = "Drop", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) },
            new { Id = 12L, Code = StoreCommerce.Domain.StaticEntity.PermissionCodes.DropEventsManage, Name = "Gerenciar drops", Description = "Gerenciar drops", Module = "Drop", IsActive = true, CreatedAt = new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
