using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "order_number",
                table: "pedido",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Pedidos existentes recebem a numeração na ordem de criação (Id), por loja, antes do índice único.
            migrationBuilder.Sql(@"
                UPDATE pedido p SET order_number = numbered.position
                FROM (
                    SELECT id, ROW_NUMBER() OVER (PARTITION BY enterprise_id ORDER BY id) AS position
                    FROM pedido
                ) numbered
                WHERE p.id = numbered.id;");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_enterprise_id_order_number",
                table: "pedido",
                columns: new[] { "enterprise_id", "order_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_pedido_enterprise_id_order_number",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "order_number",
                table: "pedido");
        }
    }
}
