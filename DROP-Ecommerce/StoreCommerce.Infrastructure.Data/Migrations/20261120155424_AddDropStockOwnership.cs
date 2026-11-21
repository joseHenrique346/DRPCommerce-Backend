using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDropStockOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_transferred_to_drop",
                table: "produto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "transferencia_estoque_drop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    snapshot_json = table.Column<string>(type: "jsonb", nullable: false),
                    drop_catalog_product_id = table.Column<long>(type: "bigint", nullable: true),
                    return_operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    imported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transferencia_estoque_drop", x => x.id);
                    table.ForeignKey(
                        name: "fk_transferencia_estoque_drop_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transferencia_estoque_drop_produto_product_id",
                        column: x => x.product_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_estoque_drop_enterprise_id_idempotency_key",
                table: "transferencia_estoque_drop",
                columns: new[] { "enterprise_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_estoque_drop_enterprise_id_product_id",
                table: "transferencia_estoque_drop",
                columns: new[] { "enterprise_id", "product_id" },
                unique: true,
                filter: "returned_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_estoque_drop_operation_id",
                table: "transferencia_estoque_drop",
                column: "operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_estoque_drop_product_id",
                table: "transferencia_estoque_drop",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transferencia_estoque_drop");

            migrationBuilder.DropColumn(
                name: "is_transferred_to_drop",
                table: "produto");
        }
    }
}
