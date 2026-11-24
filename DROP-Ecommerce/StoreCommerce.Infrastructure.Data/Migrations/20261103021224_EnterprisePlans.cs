using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnterprisePlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "plan_id",
                table: "empresa",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateTable(
                name: "plano_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    allows_vitrine = table.Column<bool>(type: "boolean", nullable: false),
                    allows_employees = table.Column<bool>(type: "boolean", nullable: false),
                    allows_custom_domain = table.Column<bool>(type: "boolean", nullable: false),
                    allows_custom_theme = table.Column<bool>(type: "boolean", nullable: false),
                    includes_drop_module = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plano_empresa", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "plano_empresa",
                columns: new[] { "id", "allows_custom_domain", "allows_custom_theme", "allows_employees", "allows_vitrine", "description", "includes_drop_module", "summary" },
                values: new object[,]
                {
                    { 1L, false, false, false, true, "Comum", false, "Vitrine padrao, sem contas de funcionario, sem dominio proprio, sem tema personalizado e sem o modulo DROP." },
                    { 2L, true, true, true, true, "Premium", false, "Inclui contas de funcionario, dominio proprio e tema personalizado, sem o modulo DROP." },
                    { 3L, true, true, true, true, "Drop", true, "Plano completo, com contas de funcionario, dominio proprio, tema personalizado e o modulo DROP." }
                });

            migrationBuilder.CreateIndex(
                name: "ix_empresa_plan_id",
                table: "empresa",
                column: "plan_id");

            migrationBuilder.AddForeignKey(
                name: "fk_empresa_plano_empresa_plan_id",
                table: "empresa",
                column: "plan_id",
                principalTable: "plano_empresa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_empresa_plano_empresa_plan_id",
                table: "empresa");

            migrationBuilder.DropTable(
                name: "plano_empresa");

            migrationBuilder.DropIndex(
                name: "ix_empresa_plan_id",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "plan_id",
                table: "empresa");
        }
    }
}
