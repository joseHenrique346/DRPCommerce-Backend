using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueCategorySlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_categoria_enterprise_id_slug",
                table: "categoria",
                columns: new[] { "enterprise_id", "slug" },
                unique: true,
                filter: "is_deleted = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categoria_enterprise_id_slug",
                table: "categoria");
        }
    }
}
