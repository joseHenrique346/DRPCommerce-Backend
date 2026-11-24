using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BindProviderAuthorizationToProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "provider",
                table: "autorizacao_provedor",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "mercado-pago");

            migrationBuilder.Sql("ALTER TABLE autorizacao_provedor ALTER COLUMN provider DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "provider",
                table: "autorizacao_provedor");
        }
    }
}
