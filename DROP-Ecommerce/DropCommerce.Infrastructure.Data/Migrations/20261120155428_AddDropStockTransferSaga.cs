using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DropCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDropStockTransferSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "source_variant_id",
                table: "catalogo_variante",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_returned",
                table: "catalogo_produto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_returning",
                table: "catalogo_produto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "source_export_id",
                table: "catalogo_produto",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_variant_ids_json",
                table: "catalogo_produto",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "devolucao_estoque_store",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    catalog_product_id = table.Column<long>(type: "bigint", nullable: false),
                    snapshot_json = table.Column<string>(type: "jsonb", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devolucao_estoque_store", x => x.id);
                    table.ForeignKey(
                        name: "fk_devolucao_estoque_store_catalogo_produto_catalog_product_id",
                        column: x => x.catalog_product_id,
                        principalTable: "catalogo_produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recebimento_estoque_store",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    catalog_product_id = table.Column<long>(type: "bigint", nullable: true),
                    accepted = table.Column<bool>(type: "boolean", nullable: false),
                    rejection_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recebimento_estoque_store", x => x.id);
                    table.ForeignKey(
                        name: "fk_recebimento_estoque_store_catalogo_produto_catalog_product_~",
                        column: x => x.catalog_product_id,
                        principalTable: "catalogo_produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devolucao_estoque_store_catalog_product_id",
                table: "devolucao_estoque_store",
                column: "catalog_product_id",
                unique: true,
                filter: "completed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_devolucao_estoque_store_operation_id",
                table: "devolucao_estoque_store",
                column: "operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recebimento_estoque_store_catalog_product_id",
                table: "recebimento_estoque_store",
                column: "catalog_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_recebimento_estoque_store_operation_id",
                table: "recebimento_estoque_store",
                column: "operation_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devolucao_estoque_store");

            migrationBuilder.DropTable(
                name: "recebimento_estoque_store");

            migrationBuilder.DropColumn(
                name: "source_variant_id",
                table: "catalogo_variante");

            migrationBuilder.DropColumn(
                name: "is_returned",
                table: "catalogo_produto");

            migrationBuilder.DropColumn(
                name: "is_returning",
                table: "catalogo_produto");

            migrationBuilder.DropColumn(
                name: "source_export_id",
                table: "catalogo_produto");

            migrationBuilder.DropColumn(
                name: "source_variant_ids_json",
                table: "catalogo_produto");
        }
    }
}
