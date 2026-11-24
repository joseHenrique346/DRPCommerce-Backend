using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseCommercialContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_empresa_plan_id",
                table: "empresa");

            migrationBuilder.AddColumn<bool>(
                name: "allows_full_customization",
                table: "plano_empresa",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "allows_logo_customization",
                table: "plano_empresa",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "plano_empresa",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "drop_product_limit",
                table: "plano_empresa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "drop_theme_limit",
                table: "plano_empresa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "employee_limit",
                table: "plano_empresa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_price",
                table: "plano_empresa",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "store_product_limit",
                table: "plano_empresa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "store_theme_limit",
                table: "plano_empresa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "add_on_id",
                table: "empresa",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "catalog_version",
                table: "empresa",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "contract_revision",
                table: "empresa",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateTable(
                name: "adicional_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    resource_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    monthly_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adicional_empresa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plano_adicional_empresa",
                columns: table => new
                {
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    add_on_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_adicional_empresa", x => new { x.plan_id, x.add_on_id });
                    table.ForeignKey(
                        name: "fk_plano_adicional_empresa_adicional_empresa_add_on_id",
                        column: x => x.add_on_id,
                        principalTable: "adicional_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plano_adicional_empresa_plano_empresa_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plano_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacao_contrato_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    requested_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    add_on_id = table.Column<long>(type: "bigint", nullable: true),
                    expected_contract_revision = table.Column<long>(type: "bigint", nullable: false),
                    catalog_version = table.Column<int>(type: "integer", nullable: false),
                    plan_monthly_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    add_on_monthly_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    superseded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitacao_contrato_empresa", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitacao_contrato_empresa_adicional_empresa_add_on_id",
                        column: x => x.add_on_id,
                        principalTable: "adicional_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacao_contrato_empresa_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacao_contrato_empresa_plano_adicional_empresa_plan_i~",
                        columns: x => new { x.plan_id, x.add_on_id },
                        principalTable: "plano_adicional_empresa",
                        principalColumns: new[] { "plan_id", "add_on_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacao_contrato_empresa_plano_empresa_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plano_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacao_contrato_empresa_usuario_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "adicional_empresa",
                columns: new[] { "id", "code", "description", "monthly_price", "quantity", "resource_code" },
                values: new object[,]
                {
                    { 1L, "team-small", "Equipe pequena", 9.90m, 1, "employees" },
                    { 2L, "team-large", "Equipe ampliada", 29.90m, 5, "employees" },
                    { 3L, "catalog-small", "Catálogo pequeno", 9.90m, 100, "store_products" },
                    { 4L, "catalog-large", "Catálogo ampliado", 19.90m, 500, "store_products" },
                    { 5L, "launches", "Lançamentos", 14.90m, 25, "drop_products" }
                });

            migrationBuilder.UpdateData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "allows_full_customization", "allows_logo_customization", "code", "description", "drop_product_limit", "drop_theme_limit", "employee_limit", "monthly_price", "store_product_limit", "store_theme_limit", "summary" },
                values: new object[] { false, false, "free", "Gratuito", 0, 0, 0, 0m, 50, 1, "Gratuito: recursos e limites por loja, conforme o catálogo comercial." });

            migrationBuilder.UpdateData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 2L,
                columns: new[] { "allows_full_customization", "allows_logo_customization", "code", "description", "drop_product_limit", "drop_theme_limit", "employee_limit", "monthly_price", "store_product_limit", "store_theme_limit", "summary" },
                values: new object[] { true, true, "pro", "Pro", 0, 0, 3, 99m, 500, 10, "Pro: recursos e limites por loja, conforme o catálogo comercial." });

            migrationBuilder.UpdateData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 3L,
                columns: new[] { "allows_full_customization", "allows_logo_customization", "code", "description", "drop_product_limit", "drop_theme_limit", "employee_limit", "monthly_price", "store_product_limit", "store_theme_limit", "summary" },
                values: new object[] { true, true, "drpcommerce", "DRPCommerce", 100, 10, 5, 199m, 2000, 20, "DRPCommerce: recursos e limites por loja, conforme o catálogo comercial." });

            migrationBuilder.InsertData(
                table: "plano_empresa",
                columns: new[] { "id", "allows_custom_domain", "allows_custom_theme", "allows_employees", "allows_full_customization", "allows_logo_customization", "allows_vitrine", "code", "description", "drop_product_limit", "drop_theme_limit", "employee_limit", "includes_drop_module", "monthly_price", "store_product_limit", "store_theme_limit", "summary" },
                values: new object[,]
                {
                    { 4L, false, true, true, false, true, true, "initial", "Inicial", 0, 0, 1, false, 49m, 100, 3, "Inicial: recursos e limites por loja, conforme o catálogo comercial." },
                    { 5L, true, true, true, true, true, false, "drpdrop", "DRPDrop", 100, 10, 5, true, 149m, 0, 0, "DRPDrop: recursos e limites por loja, conforme o catálogo comercial." }
                });

            migrationBuilder.InsertData(
                table: "plano_adicional_empresa",
                columns: new[] { "add_on_id", "plan_id" },
                values: new object[,]
                {
                    { 1L, 2L },
                    { 2L, 2L },
                    { 3L, 2L },
                    { 4L, 2L },
                    { 1L, 3L },
                    { 2L, 3L },
                    { 4L, 3L },
                    { 5L, 3L },
                    { 1L, 4L },
                    { 3L, 4L },
                    { 1L, 5L },
                    { 2L, 5L },
                    { 5L, 5L }
                });

            migrationBuilder.CreateIndex(
                name: "ix_plano_empresa_code",
                table: "plano_empresa",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empresa_add_on_id",
                table: "empresa",
                column: "add_on_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_plan_id_add_on_id",
                table: "empresa",
                columns: new[] { "plan_id", "add_on_id" });

            migrationBuilder.CreateIndex(
                name: "ix_adicional_empresa_code",
                table: "adicional_empresa",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plano_adicional_empresa_add_on_id",
                table: "plano_adicional_empresa",
                column: "add_on_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_contrato_empresa_add_on_id",
                table: "solicitacao_contrato_empresa",
                column: "add_on_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_contrato_empresa_enterprise_id_idempotency_key",
                table: "solicitacao_contrato_empresa",
                columns: new[] { "enterprise_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_contrato_empresa_plan_id_add_on_id",
                table: "solicitacao_contrato_empresa",
                columns: new[] { "plan_id", "add_on_id" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_contrato_empresa_requested_by_user_id",
                table: "solicitacao_contrato_empresa",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_solicitacao_contrato_empresa_pendente",
                table: "solicitacao_contrato_empresa",
                column: "enterprise_id",
                unique: true,
                filter: "status = 1");

            migrationBuilder.AddForeignKey(
                name: "fk_empresa_adicional_empresa_add_on_id",
                table: "empresa",
                column: "add_on_id",
                principalTable: "adicional_empresa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_empresa_plano_adicional_empresa_plan_id_add_on_id",
                table: "empresa",
                columns: new[] { "plan_id", "add_on_id" },
                principalTable: "plano_adicional_empresa",
                principalColumns: new[] { "plan_id", "add_on_id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_empresa_adicional_empresa_add_on_id",
                table: "empresa");

            migrationBuilder.DropForeignKey(
                name: "fk_empresa_plano_adicional_empresa_plan_id_add_on_id",
                table: "empresa");

            migrationBuilder.DropTable(
                name: "solicitacao_contrato_empresa");

            migrationBuilder.DropTable(
                name: "plano_adicional_empresa");

            migrationBuilder.DropTable(
                name: "adicional_empresa");

            migrationBuilder.DropIndex(
                name: "ix_plano_empresa_code",
                table: "plano_empresa");

            migrationBuilder.DropIndex(
                name: "ix_empresa_add_on_id",
                table: "empresa");

            migrationBuilder.DropIndex(
                name: "ix_empresa_plan_id_add_on_id",
                table: "empresa");

            migrationBuilder.DeleteData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 5L);

            migrationBuilder.DropColumn(
                name: "allows_full_customization",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "allows_logo_customization",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "code",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "drop_product_limit",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "drop_theme_limit",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "employee_limit",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "monthly_price",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "store_product_limit",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "store_theme_limit",
                table: "plano_empresa");

            migrationBuilder.DropColumn(
                name: "add_on_id",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "catalog_version",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "contract_revision",
                table: "empresa");

            migrationBuilder.UpdateData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "description", "summary" },
                values: new object[] { "Comum", "Vitrine padrao, sem contas de funcionario, sem dominio proprio, sem tema personalizado e sem o modulo DROP." });

            migrationBuilder.UpdateData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 2L,
                columns: new[] { "description", "summary" },
                values: new object[] { "Premium", "Inclui contas de funcionario, dominio proprio e tema personalizado, sem o modulo DROP." });

            migrationBuilder.UpdateData(
                table: "plano_empresa",
                keyColumn: "id",
                keyValue: 3L,
                columns: new[] { "description", "summary" },
                values: new object[] { "Drop", "Plano completo, com contas de funcionario, dominio proprio, tema personalizado e o modulo DROP." });

            migrationBuilder.CreateIndex(
                name: "ix_empresa_plan_id",
                table: "empresa",
                column: "plan_id");
        }
    }
}
