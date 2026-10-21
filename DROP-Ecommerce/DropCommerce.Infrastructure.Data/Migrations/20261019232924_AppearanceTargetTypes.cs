using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DropCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppearanceTargetTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_aparencia_enterprise_id_target_key",
                table: "aparencia");

            migrationBuilder.AddColumn<long>(
                name: "appearance_target_type_id",
                table: "aparencia",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tipo_alvo_aparencia",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_alvo_aparencia", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "tipo_alvo_aparencia",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { 1L, "store", "Vitrine" },
                    { 2L, "drop-default", "Padrão dos drops" },
                    { 3L, "drop-event", "Drop específico" }
                });

            migrationBuilder.Sql("""
                DO $appearance_target_types$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM aparencia
                        WHERE NOT COALESCE(
                            (target_key = 'drop-default' AND drop_event_id IS NULL) OR
                            (target_key = 'drop-event:' || drop_event_id::text AND drop_event_id > 0),
                            FALSE
                        )
                    ) THEN
                        RAISE EXCEPTION 'Existem alvos de aparência incompatíveis com o módulo Drop.';
                    END IF;
                END;
                $appearance_target_types$;

                UPDATE aparencia
                SET appearance_target_type_id = CASE WHEN target_key = 'drop-default' THEN 2 ELSE 3 END;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "appearance_target_type_id",
                table: "aparencia",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "target_key",
                table: "aparencia");

            migrationBuilder.CreateIndex(
                name: "ix_aparencia_appearance_target_type_id",
                table: "aparencia",
                column: "appearance_target_type_id");

            migrationBuilder.CreateIndex(
                name: "ux_appearance_default_target",
                table: "aparencia",
                columns: new[] { "enterprise_id", "appearance_target_type_id" },
                unique: true,
                filter: "drop_event_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_appearance_specific_target",
                table: "aparencia",
                columns: new[] { "enterprise_id", "appearance_target_type_id", "drop_event_id" },
                unique: true,
                filter: "drop_event_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_appearance_drop_target",
                table: "aparencia",
                sql: "appearance_target_type_id IN (2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_appearance_target_event",
                table: "aparencia",
                sql: "(appearance_target_type_id = 3 AND drop_event_id IS NOT NULL AND drop_event_id > 0) OR (appearance_target_type_id IN (1, 2) AND drop_event_id IS NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_alvo_aparencia_code",
                table: "tipo_alvo_aparencia",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_appearance_target_type",
                table: "aparencia",
                column: "appearance_target_type_id",
                principalTable: "tipo_alvo_aparencia",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "target_key",
                table: "aparencia",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE aparencia
                SET target_key = CASE appearance_target_type_id
                    WHEN 2 THEN 'drop-default'
                    WHEN 3 THEN 'drop-event:' || drop_event_id::text
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "target_key",
                table: "aparencia",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "fk_appearance_target_type",
                table: "aparencia");

            migrationBuilder.DropTable(
                name: "tipo_alvo_aparencia");

            migrationBuilder.DropIndex(
                name: "ix_aparencia_appearance_target_type_id",
                table: "aparencia");

            migrationBuilder.DropIndex(
                name: "ux_appearance_default_target",
                table: "aparencia");

            migrationBuilder.DropIndex(
                name: "ux_appearance_specific_target",
                table: "aparencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_appearance_drop_target",
                table: "aparencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_appearance_target_event",
                table: "aparencia");

            migrationBuilder.DropColumn(
                name: "appearance_target_type_id",
                table: "aparencia");

            migrationBuilder.CreateIndex(
                name: "ix_aparencia_enterprise_id_target_key",
                table: "aparencia",
                columns: new[] { "enterprise_id", "target_key" },
                unique: true);
        }
    }
}
