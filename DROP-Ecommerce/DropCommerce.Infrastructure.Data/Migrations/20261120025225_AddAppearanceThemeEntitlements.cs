using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DropCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppearanceThemeEntitlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "catalog_position",
                table: "versao_tema",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                WITH first_seen AS (
                    SELECT appearance_channel_id, key, MIN(id) AS first_id
                    FROM versao_tema GROUP BY appearance_channel_id, key
                ), positions AS (
                    SELECT appearance_channel_id, key,
                        ROW_NUMBER() OVER (PARTITION BY appearance_channel_id ORDER BY first_id) AS position
                    FROM first_seen
                )
                UPDATE versao_tema release SET catalog_position = positions.position::integer
                FROM positions WHERE release.appearance_channel_id = positions.appearance_channel_id AND release.key = positions.key;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "catalog_position",
                table: "versao_tema");
        }
    }
}
