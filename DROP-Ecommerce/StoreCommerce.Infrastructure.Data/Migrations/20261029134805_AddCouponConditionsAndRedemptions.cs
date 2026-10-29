using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponConditionsAndRedemptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operador_condicao_cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operador_condicao_cupom", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resgate_cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    coupon_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    redeemed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    released_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resgate_cupom", x => x.id);
                    table.ForeignKey(
                        name: "fk_resgate_cupom_cliente_customer_id",
                        column: x => x.customer_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resgate_cupom_cupom_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resgate_cupom_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_condicao_cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_condicao_cupom", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "condicao_cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    coupon_condition_type_id = table.Column<long>(type: "bigint", nullable: false),
                    coupon_condition_operator_id = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    second_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    coupon_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_condicao_cupom", x => x.id);
                    table.ForeignKey(
                        name: "fk_condicao_cupom_cupom_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_condicao_cupom_operador_condicao_cupom_coupon_condition_ope~",
                        column: x => x.coupon_condition_operator_id,
                        principalTable: "operador_condicao_cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_condicao_cupom_tipo_condicao_cupom_coupon_condition_type_id",
                        column: x => x.coupon_condition_type_id,
                        principalTable: "tipo_condicao_cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "operador_condicao_cupom",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Igual a" },
                    { 2L, "Maior que" },
                    { 3L, "Maior ou igual a" },
                    { 4L, "Menor que" },
                    { 5L, "Menor ou igual a" },
                    { 6L, "Entre (inclusive)" },
                    { 7L, "Maior que e menor que" }
                });

            migrationBuilder.InsertData(
                table: "tipo_condicao_cupom",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Valor do pedido" },
                    { 2L, "Quantidade de itens do pedido" },
                    { 3L, "Compras anteriores do cliente" },
                    { 4L, "Total já gasto pelo cliente" }
                });

            // O pedido mínimo do cupom vira a condição "valor do pedido (1) maior ou igual a (3)".
            migrationBuilder.Sql(@"
                INSERT INTO condicao_cupom (coupon_id, coupon_condition_type_id, coupon_condition_operator_id, value, second_value)
                SELECT id, 1, 3, min_order_value, NULL FROM cupom WHERE min_order_value > 0;");

            migrationBuilder.DropColumn(
                name: "min_order_value",
                table: "cupom");

            // Cupons de valor fixo não usam teto de desconto.
            migrationBuilder.Sql("UPDATE cupom SET max_discount_cap = 0 WHERE coupon_type_id = 2;");

            // Códigos passam a ser gravados sem espaços e em maiúsculas. Repetições na mesma loja (entre cupons não excluídos)
            // recebem o id como sufixo para que o índice único possa ser criado.
            migrationBuilder.Sql(@"
                UPDATE cupom SET code = UPPER(REGEXP_REPLACE(code, '\s+', '', 'g'));

                UPDATE cupom c
                SET code = LEFT(c.code, 50 - LENGTH(c.id::text) - 1) || '-' || c.id::text
                FROM (
                    SELECT id, ROW_NUMBER() OVER (PARTITION BY enterprise_id, code ORDER BY id) AS position
                    FROM cupom
                    WHERE is_deleted = FALSE
                ) duplicated
                WHERE c.id = duplicated.id AND duplicated.position > 1;");

            migrationBuilder.CreateIndex(
                name: "ix_cupom_enterprise_id_code",
                table: "cupom",
                columns: new[] { "enterprise_id", "code" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_condicao_cupom_coupon_condition_operator_id",
                table: "condicao_cupom",
                column: "coupon_condition_operator_id");

            migrationBuilder.CreateIndex(
                name: "ix_condicao_cupom_coupon_condition_type_id",
                table: "condicao_cupom",
                column: "coupon_condition_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_condicao_cupom_coupon_id",
                table: "condicao_cupom",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "ix_resgate_cupom_coupon_id_customer_id",
                table: "resgate_cupom",
                columns: new[] { "coupon_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "ix_resgate_cupom_customer_id",
                table: "resgate_cupom",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_resgate_cupom_enterprise_id",
                table: "resgate_cupom",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_resgate_cupom_order_id",
                table: "resgate_cupom",
                column: "order_id",
                unique: true,
                filter: "released_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "min_order_value",
                table: "cupom",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
                UPDATE cupom c SET min_order_value = cc.value
                FROM condicao_cupom cc
                WHERE cc.coupon_id = c.id AND cc.coupon_condition_type_id = 1 AND cc.coupon_condition_operator_id = 3;");

            migrationBuilder.DropTable(
                name: "condicao_cupom");

            migrationBuilder.DropTable(
                name: "resgate_cupom");

            migrationBuilder.DropTable(
                name: "operador_condicao_cupom");

            migrationBuilder.DropTable(
                name: "tipo_condicao_cupom");

            migrationBuilder.DropIndex(
                name: "ix_cupom_enterprise_id_code",
                table: "cupom");
        }
    }
}
