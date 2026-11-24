using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DropCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDropCommercialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "product_id",
                table: "produto",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "catalog_product_id",
                table: "produto",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_contract_blocked",
                table: "produto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<long>(
                name: "product_id",
                table: "evento",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "atividade_comercial",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    catalog_product_id = table.Column<long>(type: "bigint", nullable: true),
                    participation_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_atividade_comercial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalogo_produto",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    store_product_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    is_contract_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalogo_produto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contrato_comercial",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    catalog_version = table.Column<int>(type: "integer", nullable: false),
                    allows_drop = table.Column<bool>(type: "boolean", nullable: false),
                    product_limit = table.Column<int>(type: "integer", nullable: false),
                    applied_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contrato_comercial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalogo_variante",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    catalog_product_id = table.Column<long>(type: "bigint", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    options_json = table.Column<string>(type: "jsonb", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    quantity_on_hand = table.Column<int>(type: "integer", nullable: false),
                    quantity_reserved = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalogo_variante", x => x.id);
                    table.ForeignKey(
                        name: "fk_catalogo_variante_catalogo_produto_catalog_product_id",
                        column: x => x.catalog_product_id,
                        principalTable: "catalogo_produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produto_catalog_product_id",
                table: "produto",
                column: "catalog_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_drop_event_id_catalog_product_id",
                table: "produto",
                columns: new[] { "drop_event_id", "catalog_product_id" });

            migrationBuilder.CreateIndex(
                name: "ix_atividade_comercial_enterprise_id_id",
                table: "atividade_comercial",
                columns: new[] { "enterprise_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_catalogo_produto_enterprise_id_store_product_id",
                table: "catalogo_produto",
                columns: new[] { "enterprise_id", "store_product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_catalogo_variante_catalog_product_id_sku",
                table: "catalogo_variante",
                columns: new[] { "catalog_product_id", "sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contrato_comercial_enterprise_id",
                table: "contrato_comercial",
                column: "enterprise_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_produto_catalogo_produto_catalog_product_id",
                table: "produto",
                column: "catalog_product_id",
                principalTable: "catalogo_produto",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                INSERT INTO catalogo_produto
                    (enterprise_id, store_product_id, name, description, is_contract_blocked, is_deleted, created_at)
                SELECT e.enterprise_id, p.product_id, 'Produto Store ' || p.product_id,
                    'Vínculo legado; conferir transferência de estoque antes de publicar.', FALSE, FALSE, MIN(p.created_at)
                FROM produto p JOIN evento e ON e.id = p.drop_event_id
                WHERE p.product_id IS NOT NULL
                GROUP BY e.enterprise_id, p.product_id;

                UPDATE produto p SET catalog_product_id = c.id
                FROM evento e, catalogo_produto c
                WHERE e.id = p.drop_event_id AND c.enterprise_id = e.enterprise_id AND c.store_product_id = p.product_id;

                INSERT INTO catalogo_variante
                    (catalog_product_id, sku, options_json, price, quantity_on_hand, quantity_reserved, created_at)
                SELECT DISTINCT ON (p.catalog_product_id, p.sku)
                    p.catalog_product_id, p.sku, '{}'::jsonb, p.price, 0, 0, p.created_at
                FROM produto p WHERE p.catalog_product_id IS NOT NULL
                ORDER BY p.catalog_product_id, p.sku, p.created_at DESC, p.id DESC;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM catalogo_produto WHERE store_product_id IS NULL) THEN
                        RAISE EXCEPTION 'Produtos próprios DROP impedem rollback para ProductId obrigatório.';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropForeignKey(
                name: "fk_produto_catalogo_produto_catalog_product_id",
                table: "produto");

            migrationBuilder.DropTable(
                name: "atividade_comercial");

            migrationBuilder.DropTable(
                name: "catalogo_variante");

            migrationBuilder.DropTable(
                name: "contrato_comercial");

            migrationBuilder.DropTable(
                name: "catalogo_produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_catalog_product_id",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_drop_event_id_catalog_product_id",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "catalog_product_id",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "is_contract_blocked",
                table: "produto");

            migrationBuilder.AlterColumn<long>(
                name: "product_id",
                table: "produto",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "product_id",
                table: "evento",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
