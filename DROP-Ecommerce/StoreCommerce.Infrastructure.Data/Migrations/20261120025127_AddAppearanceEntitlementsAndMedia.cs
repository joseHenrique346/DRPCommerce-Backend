using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppearanceEntitlementsAndMedia : Migration
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

            migrationBuilder.CreateTable(
                name: "ativo_midia",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    byte_length = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    content = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ativo_midia", x => x.id);
                    table.ForeignKey(
                        name: "fk_ativo_midia_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ativo_midia_enterprise_id_content_hash",
                table: "ativo_midia",
                columns: new[] { "enterprise_id", "content_hash" },
                unique: true,
                filter: "is_deleted = FALSE");

            migrationBuilder.CreateIndex(
                name: "ix_ativo_midia_public_id",
                table: "ativo_midia",
                column: "public_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ativo_midia");

            migrationBuilder.DropColumn(
                name: "catalog_position",
                table: "versao_tema");
        }
    }
}
