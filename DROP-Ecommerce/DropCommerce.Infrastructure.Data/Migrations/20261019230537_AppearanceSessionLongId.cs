using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DropCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppearanceSessionLongId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_sessao_edicao_aparencia",
                table: "sessao_edicao_aparencia");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "sessao_edicao_aparencia",
                newName: "legacy_id");

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "sessao_edicao_aparencia",
                type: "bigint",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "pk_sessao_edicao_aparencia",
                table: "sessao_edicao_aparencia",
                column: "id");

            migrationBuilder.DropColumn(
                name: "legacy_id",
                table: "sessao_edicao_aparencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_sessao_edicao_aparencia",
                table: "sessao_edicao_aparencia");

            migrationBuilder.DropColumn(
                name: "id",
                table: "sessao_edicao_aparencia");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "sessao_edicao_aparencia",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "sessao_edicao_aparencia",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sessao_edicao_aparencia",
                table: "sessao_edicao_aparencia",
                column: "id");
        }
    }
}
