using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Appearance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "versao_tema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    channel = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    renderer_contract = table.Column<int>(type: "integer", nullable: false),
                    schema_json = table.Column<string>(type: "jsonb", nullable: false),
                    defaults_json = table.Column<string>(type: "jsonb", nullable: false),
                    checksum = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    migration_deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replacement_release_id = table.Column<long>(type: "bigint", nullable: true),
                    migration_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_versao_tema", x => x.id);
                    table.ForeignKey(
                        name: "fk_theme_replacement",
                        column: x => x.replacement_release_id,
                        principalTable: "versao_tema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aparencia",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    target_key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    resource_id = table.Column<long>(type: "bigint", nullable: true),
                    published_revision_id = table.Column<long>(type: "bigint", nullable: true),
                    migration_blocked = table.Column<bool>(type: "boolean", nullable: false),
                    migration_failure_code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aparencia", x => x.id);
                    table.UniqueConstraint("ak_appearance_tenant_id", x => new { x.enterprise_id, x.id });
                    table.ForeignKey(
                        name: "fk_aparencia_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "revisao_aparencia",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    appearance_id = table.Column<long>(type: "bigint", nullable: false),
                    theme_release_id = table.Column<long>(type: "bigint", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false),
                    settings_json = table.Column<string>(type: "jsonb", nullable: false),
                    checksum = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    published_by = table.Column<long>(type: "bigint", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    restored_from_revision_id = table.Column<long>(type: "bigint", nullable: true),
                    origin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_revisao_aparencia", x => x.id);
                    table.UniqueConstraint("ak_revision_tenant_id", x => new { x.enterprise_id, x.id });
                    table.UniqueConstraint("ak_revision_tenant_owner_id", x => new { x.enterprise_id, x.appearance_id, x.id });
                    table.ForeignKey(
                        name: "fk_revision_appearance",
                        columns: x => new { x.enterprise_id, x.appearance_id },
                        principalTable: "aparencia",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_release",
                        column: x => x.theme_release_id,
                        principalTable: "versao_tema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_restored_from",
                        columns: x => new { x.enterprise_id, x.restored_from_revision_id },
                        principalTable: "revisao_aparencia",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessao_edicao_aparencia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    appearance_id = table.Column<long>(type: "bigint", nullable: false),
                    theme_release_id = table.Column<long>(type: "bigint", nullable: false),
                    editor_id = table.Column<long>(type: "bigint", nullable: false),
                    base_revision_id = table.Column<long>(type: "bigint", nullable: true),
                    restored_from_revision_id = table.Column<long>(type: "bigint", nullable: true),
                    settings_json = table.Column<string>(type: "jsonb", nullable: false),
                    preview_json = table.Column<string>(type: "jsonb", nullable: true),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_open = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessao_edicao_aparencia", x => x.id);
                    table.ForeignKey(
                        name: "fk_edit_appearance",
                        columns: x => new { x.enterprise_id, x.appearance_id },
                        principalTable: "aparencia",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_edit_base_revision",
                        columns: x => new { x.enterprise_id, x.appearance_id, x.base_revision_id },
                        principalTable: "revisao_aparencia",
                        principalColumns: new[] { "enterprise_id", "appearance_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_edit_release",
                        column: x => x.theme_release_id,
                        principalTable: "versao_tema",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_edit_restore_revision",
                        columns: x => new { x.enterprise_id, x.appearance_id, x.restored_from_revision_id },
                        principalTable: "revisao_aparencia",
                        principalColumns: new[] { "enterprise_id", "appearance_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aparencia_enterprise_id_id_published_revision_id",
                table: "aparencia",
                columns: new[] { "enterprise_id", "id", "published_revision_id" });

            migrationBuilder.CreateIndex(
                name: "ix_aparencia_enterprise_id_target_key",
                table: "aparencia",
                columns: new[] { "enterprise_id", "target_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_revisao_aparencia_appearance_id_number",
                table: "revisao_aparencia",
                columns: new[] { "appearance_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_revisao_aparencia_enterprise_id_restored_from_revision_id",
                table: "revisao_aparencia",
                columns: new[] { "enterprise_id", "restored_from_revision_id" });

            migrationBuilder.CreateIndex(
                name: "ix_revisao_aparencia_theme_release_id",
                table: "revisao_aparencia",
                column: "theme_release_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessao_edicao_aparencia_appearance_id",
                table: "sessao_edicao_aparencia",
                column: "appearance_id",
                unique: true,
                filter: "is_open = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_sessao_edicao_aparencia_enterprise_id_appearance_id_base_re~",
                table: "sessao_edicao_aparencia",
                columns: new[] { "enterprise_id", "appearance_id", "base_revision_id" });

            migrationBuilder.CreateIndex(
                name: "ix_sessao_edicao_aparencia_enterprise_id_appearance_id_restore~",
                table: "sessao_edicao_aparencia",
                columns: new[] { "enterprise_id", "appearance_id", "restored_from_revision_id" });

            migrationBuilder.CreateIndex(
                name: "ix_sessao_edicao_aparencia_is_open_expires_at",
                table: "sessao_edicao_aparencia",
                columns: new[] { "is_open", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_sessao_edicao_aparencia_theme_release_id",
                table: "sessao_edicao_aparencia",
                column: "theme_release_id");

            migrationBuilder.CreateIndex(
                name: "ix_versao_tema_channel_key_version",
                table: "versao_tema",
                columns: new[] { "channel", "key", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_versao_tema_replacement_release_id",
                table: "versao_tema",
                column: "replacement_release_id");

            migrationBuilder.AddForeignKey(
                name: "fk_appearance_published_revision",
                table: "aparencia",
                columns: new[] { "enterprise_id", "id", "published_revision_id" },
                principalTable: "revisao_aparencia",
                principalColumns: new[] { "enterprise_id", "appearance_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_appearance_published_revision",
                table: "aparencia");

            migrationBuilder.DropTable(
                name: "sessao_edicao_aparencia");

            migrationBuilder.DropTable(
                name: "revisao_aparencia");

            migrationBuilder.DropTable(
                name: "aparencia");

            migrationBuilder.DropTable(
                name: "versao_tema");
        }
    }
}
