using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantsAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $catalog_migration$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM produto
                        WHERE is_deleted = FALSE
                        GROUP BY enterprise_id, slug HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Corrija os slugs de produto duplicados por loja antes de aplicar a migration.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM produto
                        WHERE is_deleted = FALSE
                        GROUP BY enterprise_id, UPPER(BTRIM(sku)) HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Corrija os SKUs duplicados por loja, considerando trim e letras maiusculas.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM produto WHERE BTRIM(sku) = '' OR LENGTH(UPPER(BTRIM(sku))) > 100) THEN
                        RAISE EXCEPTION 'Corrija os SKUs vazios ou maiores que 100 caracteres antes da migration.';
                    END IF;
                END
                $catalog_migration$;

                UPDATE produto SET sku = UPPER(BTRIM(sku));
                """);

            migrationBuilder.AddColumn<bool>(
                name: "has_variants",
                table: "produto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "item_name",
                table: "item_pedido",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<long>(
                name: "product_variant_id",
                table: "item_pedido",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "variant_attributes_json",
                table: "item_pedido",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "variant_name",
                table: "item_pedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_produto_enterprise_id_id",
                table: "produto",
                columns: new[] { "enterprise_id", "id" });

            migrationBuilder.CreateTable(
                name: "reserva_estoque",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reserva_estoque", x => x.id);
                    table.UniqueConstraint("ak_reserva_estoque_enterprise_id_id", x => new { x.enterprise_id, x.id });
                    table.ForeignKey(
                        name: "fk_reserva_estoque_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "variante_produto",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bar_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    attributes_json = table.Column<string>(type: "jsonb", nullable: false),
                    combination_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    cost_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    height = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    width = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    length = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false),
                    reserved_quantity = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_variante_produto", x => x.id);
                    table.UniqueConstraint("ak_variante_produto_enterprise_id_id", x => new { x.enterprise_id, x.id });
                    table.UniqueConstraint("ak_variante_produto_product_id_id", x => new { x.product_id, x.id });
                    table.CheckConstraint("ck_variante_reservado", "reserved_quantity >= 0");
                    table.CheckConstraint("ck_variante_versao", "version > 0");
                    table.ForeignKey(
                        name: "fk_variante_produto_produto_enterprise_id_product_id",
                        columns: x => new { x.enterprise_id, x.product_id },
                        principalTable: "produto",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_reserva_estoque",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    stock_reservation_id = table.Column<long>(type: "bigint", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    allows_backorder = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_reserva_estoque", x => x.id);
                    table.CheckConstraint("ck_reserva_quantidade", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_item_reserva_estoque_reserva_estoque_enterprise_id_stock_re~",
                        columns: x => new { x.enterprise_id, x.stock_reservation_id },
                        principalTable: "reserva_estoque",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_item_reserva_estoque_variante_produto_enterprise_id_product~",
                        columns: x => new { x.enterprise_id, x.product_variant_id },
                        principalTable: "variante_produto",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacao_estoque",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    stock_reservation_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    operation_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity_change = table.Column<int>(type: "integer", nullable: false),
                    reserved_change = table.Column<int>(type: "integer", nullable: false),
                    stock_after = table.Column<int>(type: "integer", nullable: false),
                    reserved_after = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    actor_user_id = table.Column<long>(type: "bigint", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movimentacao_estoque", x => x.id);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_reserva_estoque_enterprise_id_stock_re~",
                        columns: x => new { x.enterprise_id, x.stock_reservation_id },
                        principalTable: "reserva_estoque",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movimentacao_estoque_variante_produto_enterprise_id_product~",
                        columns: x => new { x.enterprise_id, x.product_variant_id },
                        principalTable: "variante_produto",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produto_enterprise_id_sku",
                table: "produto",
                columns: new[] { "enterprise_id", "sku" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_produto_enterprise_id_slug",
                table: "produto",
                columns: new[] { "enterprise_id", "slug" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_product_id_product_variant_id",
                table: "item_pedido",
                columns: new[] { "product_id", "product_variant_id" });

            migrationBuilder.CreateIndex(
                name: "ix_item_reserva_estoque_enterprise_id_product_variant_id",
                table: "item_reserva_estoque",
                columns: new[] { "enterprise_id", "product_variant_id" });

            migrationBuilder.CreateIndex(
                name: "ix_item_reserva_estoque_enterprise_id_stock_reservation_id",
                table: "item_reserva_estoque",
                columns: new[] { "enterprise_id", "stock_reservation_id" });

            migrationBuilder.CreateIndex(
                name: "ix_item_reserva_estoque_stock_reservation_id_product_variant_id",
                table: "item_reserva_estoque",
                columns: new[] { "stock_reservation_id", "product_variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_enterprise_id_occurred_at",
                table: "movimentacao_estoque",
                columns: new[] { "enterprise_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_enterprise_id_product_variant_id_type_~",
                table: "movimentacao_estoque",
                columns: new[] { "enterprise_id", "product_variant_id", "type", "operation_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimentacao_estoque_enterprise_id_stock_reservation_id",
                table: "movimentacao_estoque",
                columns: new[] { "enterprise_id", "stock_reservation_id" });

            migrationBuilder.CreateIndex(
                name: "ix_reserva_estoque_enterprise_id_idempotency_key",
                table: "reserva_estoque",
                columns: new[] { "enterprise_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reserva_estoque_status_expires_at_enterprise_id",
                table: "reserva_estoque",
                columns: new[] { "status", "expires_at", "enterprise_id" });

            migrationBuilder.CreateIndex(
                name: "ix_variante_produto_enterprise_id_product_id",
                table: "variante_produto",
                columns: new[] { "enterprise_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "ix_variante_produto_enterprise_id_sku",
                table: "variante_produto",
                columns: new[] { "enterprise_id", "sku" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_variante_produto_product_id",
                table: "variante_produto",
                column: "product_id",
                unique: true,
                filter: "is_default = TRUE AND is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_variante_produto_product_id_combination_key",
                table: "variante_produto",
                columns: new[] { "product_id", "combination_key" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.AddForeignKey(
                name: "fk_item_pedido_variante_produto_product_id_product_variant_id",
                table: "item_pedido",
                columns: new[] { "product_id", "product_variant_id" },
                principalTable: "variante_produto",
                principalColumns: new[] { "product_id", "id" },
                onDelete: ReferentialAction.Restrict);

            // Produtos anteriores ganham um SKU padrão sem inventar quantidades de estoque.
            // As referências de itens antigos são preenchidas sem alterar seus snapshots de nome, SKU e preço.
            migrationBuilder.Sql("""
                INSERT INTO variante_produto (
                    enterprise_id, product_id, name, sku, bar_code, attributes_json, combination_key,
                    is_default, is_active, stock_quantity, reserved_quantity, version,
                    is_deleted, deleted_at, created_at, updated_at
                )
                SELECT enterprise_id, id, 'Padrão', sku, COALESCE(bar_code, ''), '{}'::jsonb,
                    '44136FA355B3678A1146AD16F7E8649E94FB4FC21FE77E8310C060F61CAAFF8A',
                    TRUE, TRUE, 0, 0, 1, is_deleted, deleted_at, created_at, updated_at
                FROM produto;

                UPDATE item_pedido AS item
                SET product_variant_id = variant.id,
                    variant_name = 'Padrão',
                    variant_attributes_json = '{}'::jsonb
                FROM variante_produto AS variant
                WHERE item.product_id = variant.product_id AND variant.is_default = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_pedido_variante_produto_product_id_product_variant_id",
                table: "item_pedido");

            migrationBuilder.DropTable(
                name: "item_reserva_estoque");

            migrationBuilder.DropTable(
                name: "movimentacao_estoque");

            migrationBuilder.DropTable(
                name: "reserva_estoque");

            migrationBuilder.DropTable(
                name: "variante_produto");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_produto_enterprise_id_id",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_enterprise_id_sku",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_produto_enterprise_id_slug",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "ix_item_pedido_product_id_product_variant_id",
                table: "item_pedido");

            migrationBuilder.DropColumn(
                name: "has_variants",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "product_variant_id",
                table: "item_pedido");

            migrationBuilder.DropColumn(
                name: "variant_attributes_json",
                table: "item_pedido");

            migrationBuilder.DropColumn(
                name: "variant_name",
                table: "item_pedido");

            migrationBuilder.AlterColumn<string>(
                name: "item_name",
                table: "item_pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
