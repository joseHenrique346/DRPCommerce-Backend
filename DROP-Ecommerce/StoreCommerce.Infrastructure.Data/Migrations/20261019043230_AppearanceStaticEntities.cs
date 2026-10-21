using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppearanceStaticEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_versao_tema_channel_key_version",
                table: "versao_tema");

            migrationBuilder.AddColumn<long>(
                name: "appearance_channel_id",
                table: "versao_tema",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "appearance_edit_session_status_id",
                table: "sessao_edicao_aparencia",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "appearance_revision_origin_id",
                table: "revisao_aparencia",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "canal_aparencia",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canal_aparencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "origem_revisao_aparencia",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_origem_revisao_aparencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_sessao_edicao_aparencia",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_sessao_edicao_aparencia", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "canal_aparencia",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 1L, "store", "Vitrine" },
                    { 2L, "drop", "Drop" }
                });

            migrationBuilder.InsertData(
                table: "origem_revisao_aparencia",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 1L, "Publish", "Publicação" },
                    { 2L, "Rollback", "Restauração" },
                    { 3L, "Migration", "Migração do tema" }
                });

            migrationBuilder.InsertData(
                table: "status_sessao_edicao_aparencia",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 1L, "InEdit", "Em edição" },
                    { 2L, "InPreview", "Em prévia" },
                    { 3L, "Published", "Publicada" },
                    { 4L, "Cancelled", "Cancelada" },
                    { 5L, "Released", "Liberada pelo proprietário" },
                    { 6L, "Expired", "Expirada" },
                    { 7L, "Migrated", "Encerrada por migração" }
                });

            migrationBuilder.Sql("""
                UPDATE versao_tema AS tema
                SET appearance_channel_id = canal.id
                FROM canal_aparencia AS canal
                WHERE tema.channel = canal.code;

                UPDATE sessao_edicao_aparencia AS sessao
                SET appearance_edit_session_status_id = status.id
                FROM status_sessao_edicao_aparencia AS status
                WHERE sessao.state = status.code;

                UPDATE revisao_aparencia AS revisao
                SET appearance_revision_origin_id = origem.id
                FROM origem_revisao_aparencia AS origem
                WHERE revisao.origin = origem.code;

                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM versao_tema WHERE appearance_channel_id IS NULL)
                        OR EXISTS (SELECT 1 FROM sessao_edicao_aparencia WHERE appearance_edit_session_status_id IS NULL)
                        OR EXISTS (SELECT 1 FROM revisao_aparencia WHERE appearance_revision_origin_id IS NULL) THEN
                        RAISE EXCEPTION 'Existe uma classificacao de aparencia desconhecida. Corrija os dados antes de migrar.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "appearance_channel_id",
                table: "versao_tema",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "appearance_edit_session_status_id",
                table: "sessao_edicao_aparencia",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "appearance_revision_origin_id",
                table: "revisao_aparencia",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "channel",
                table: "versao_tema");

            migrationBuilder.DropColumn(
                name: "state",
                table: "sessao_edicao_aparencia");

            migrationBuilder.DropColumn(
                name: "origin",
                table: "revisao_aparencia");

            migrationBuilder.CreateIndex(
                name: "ix_versao_tema_appearance_channel_id_key_version",
                table: "versao_tema",
                columns: new[] { "appearance_channel_id", "key", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sessao_edicao_aparencia_appearance_edit_session_status_id",
                table: "sessao_edicao_aparencia",
                column: "appearance_edit_session_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_revisao_aparencia_appearance_revision_origin_id",
                table: "revisao_aparencia",
                column: "appearance_revision_origin_id");

            migrationBuilder.CreateIndex(
                name: "ix_canal_aparencia_code",
                table: "canal_aparencia",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_origem_revisao_aparencia_code",
                table: "origem_revisao_aparencia",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_status_sessao_edicao_aparencia_code",
                table: "status_sessao_edicao_aparencia",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_revision_origin",
                table: "revisao_aparencia",
                column: "appearance_revision_origin_id",
                principalTable: "origem_revisao_aparencia",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_edit_session_status",
                table: "sessao_edicao_aparencia",
                column: "appearance_edit_session_status_id",
                principalTable: "status_sessao_edicao_aparencia",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_theme_channel",
                table: "versao_tema",
                column: "appearance_channel_id",
                principalTable: "canal_aparencia",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_revision_origin",
                table: "revisao_aparencia");

            migrationBuilder.DropForeignKey(
                name: "fk_edit_session_status",
                table: "sessao_edicao_aparencia");

            migrationBuilder.DropForeignKey(
                name: "fk_theme_channel",
                table: "versao_tema");

            migrationBuilder.DropIndex(
                name: "ix_versao_tema_appearance_channel_id_key_version",
                table: "versao_tema");

            migrationBuilder.DropIndex(
                name: "ix_sessao_edicao_aparencia_appearance_edit_session_status_id",
                table: "sessao_edicao_aparencia");

            migrationBuilder.DropIndex(
                name: "ix_revisao_aparencia_appearance_revision_origin_id",
                table: "revisao_aparencia");

            migrationBuilder.AddColumn<string>(
                name: "channel",
                table: "versao_tema",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "sessao_edicao_aparencia",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origin",
                table: "revisao_aparencia",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE versao_tema AS tema
                SET channel = canal.code
                FROM canal_aparencia AS canal
                WHERE tema.appearance_channel_id = canal.id;

                UPDATE sessao_edicao_aparencia AS sessao
                SET state = status.code
                FROM status_sessao_edicao_aparencia AS status
                WHERE sessao.appearance_edit_session_status_id = status.id;

                UPDATE revisao_aparencia AS revisao
                SET origin = origem.code
                FROM origem_revisao_aparencia AS origem
                WHERE revisao.appearance_revision_origin_id = origem.id;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "channel",
                table: "versao_tema",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "state",
                table: "sessao_edicao_aparencia",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "origin",
                table: "revisao_aparencia",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "appearance_channel_id",
                table: "versao_tema");

            migrationBuilder.DropColumn(
                name: "appearance_edit_session_status_id",
                table: "sessao_edicao_aparencia");

            migrationBuilder.DropColumn(
                name: "appearance_revision_origin_id",
                table: "revisao_aparencia");

            migrationBuilder.DropTable(
                name: "canal_aparencia");

            migrationBuilder.DropTable(
                name: "origem_revisao_aparencia");

            migrationBuilder.DropTable(
                name: "status_sessao_edicao_aparencia");

            migrationBuilder.CreateIndex(
                name: "ix_versao_tema_channel_key_version",
                table: "versao_tema",
                columns: new[] { "channel", "key", "version" },
                unique: true);
        }
    }
}
