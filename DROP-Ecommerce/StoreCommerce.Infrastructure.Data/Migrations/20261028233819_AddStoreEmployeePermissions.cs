using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreEmployeePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permissao",
                columns: new[] { "id", "code", "created_at", "description", "is_active", "module", "name", "updated_at" },
                values: new object[,]
                {
                    { 13L, "categories.view", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar categorias", true, "Categories", "Consultar categorias", null },
                    { 14L, "categories.manage", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Gerenciar categorias", true, "Categories", "Gerenciar categorias", null },
                    { 15L, "coupons.view", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar cupons", true, "Coupons", "Consultar cupons", null },
                    { 16L, "coupons.manage", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Gerenciar cupons", true, "Coupons", "Gerenciar cupons", null },
                    { 17L, "suppliers.manage", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar e gerenciar fornecedores", true, "Suppliers", "Gerenciar fornecedores", null },
                    { 18L, "inventory.manage", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar e ajustar estoque", true, "Inventory", "Gerenciar estoque", null },
                    { 19L, "orders.cancel", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Cancelar pedidos", true, "Orders", "Cancelar pedidos", null },
                    { 20L, "orders.refund", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Registrar reembolsos de pedidos", true, "Orders", "Reembolsar pedidos", null },
                    { 21L, "settings.manage", new DateTime(2026, 10, 28, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar e alterar configurações da loja", true, "Settings", "Gerenciar configurações da loja", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 21L);
        }
    }
}
