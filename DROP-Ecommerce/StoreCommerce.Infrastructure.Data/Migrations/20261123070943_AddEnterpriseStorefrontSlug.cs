using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseStorefrontSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "storefront_slug",
                table: "empresa",
                type: "character varying(63)",
                maxLength: 63,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE empresa SET storefront_slug = 'loja-' || id
                WHERE storefront_slug IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_empresa_storefront_slug",
                table: "empresa",
                column: "storefront_slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_empresa_storefront_slug",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "storefront_slug",
                table: "empresa");
        }
    }
}
