using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductContractLimitsAndActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "contract_block_reason",
                table: "produto",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "contract_blocked_at",
                table: "produto",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_contract_blocked",
                table: "produto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "atividade_loja",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: true),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: true),
                    actor_user_id = table.Column<long>(type: "bigint", nullable: true),
                    actor_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    event_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    details_json = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_atividade_loja", x => x.id);
                    table.ForeignKey(
                        name: "fk_atividade_loja_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_atividade_loja_produto_product_id",
                        column: x => x.product_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_atividade_loja_usuario_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_atividade_loja_variante_produto_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "variante_produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permissao",
                columns: new[] { "id", "code", "created_at", "description", "is_active", "module", "name", "updated_at" },
                values: new object[] { 22L, "store.history.view", new DateTime(2026, 11, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar atividades e mudanças comerciais da própria loja", true, "StoreHistory", "Consultar histórico da loja", null });

            migrationBuilder.CreateIndex(
                name: "ix_produto_enterprise_id_is_contract_blocked_created_at_id",
                table: "produto",
                columns: new[] { "enterprise_id", "is_contract_blocked", "created_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_atividade_loja_actor_user_id",
                table: "atividade_loja",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_atividade_loja_enterprise_id_event_type_occurred_at",
                table: "atividade_loja",
                columns: new[] { "enterprise_id", "event_type", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_atividade_loja_enterprise_id_id",
                table: "atividade_loja",
                columns: new[] { "enterprise_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_atividade_loja_enterprise_id_product_id_id",
                table: "atividade_loja",
                columns: new[] { "enterprise_id", "product_id", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_atividade_loja_product_id",
                table: "atividade_loja",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_atividade_loja_product_variant_id",
                table: "atividade_loja",
                column: "product_variant_id");

            // Preserva publicação, saldos e pedidos; somente retira do catálogo o que excede o contrato.
            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT p.id,
                        pl.allows_vitrine,
                        p.is_active,
                        ROW_NUMBER() OVER (
                            PARTITION BY p.enterprise_id, p.is_active
                            ORDER BY p.created_at, p.id) AS position,
                        pl.store_product_limit + CASE
                            WHEN a.resource_code = 'store_products' THEN a.quantity ELSE 0
                        END AS product_limit
                    FROM produto p
                    JOIN empresa e ON e.id = p.enterprise_id
                    JOIN plano_empresa pl ON pl.id = e.plan_id
                    LEFT JOIN adicional_empresa a ON a.id = e.add_on_id
                    WHERE NOT p.is_deleted
                ), blocked AS (
                    UPDATE produto p
                    SET is_contract_blocked = TRUE,
                        contract_block_reason = CASE WHEN r.allows_vitrine
                            THEN 'product_limit' ELSE 'store_module' END,
                        contract_blocked_at = CURRENT_TIMESTAMP
                    FROM ranked r
                    WHERE p.id = r.id AND (NOT r.allows_vitrine OR
                        r.is_active AND r.position > r.product_limit)
                    RETURNING p.*
                )
                INSERT INTO atividade_loja
                    (enterprise_id, product_id, actor_type, event_type, reason_code,
                        correlation_id, product_name, sku, details_json, occurred_at)
                SELECT enterprise_id, id, 'system', 'product_contract_blocked', contract_block_reason,
                    'migration:20261119235550', name, sku,
                    '{"source":"contract_backfill"}'::jsonb, CURRENT_TIMESTAMP
                FROM blocked;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atividade_loja");

            migrationBuilder.DropIndex(
                name: "ix_produto_enterprise_id_is_contract_blocked_created_at_id",
                table: "produto");

            migrationBuilder.DeleteData(
                table: "permissao",
                keyColumn: "id",
                keyValue: 22L);

            migrationBuilder.DropColumn(
                name: "contract_block_reason",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "contract_blocked_at",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "is_contract_blocked",
                table: "produto");
        }
    }
}
