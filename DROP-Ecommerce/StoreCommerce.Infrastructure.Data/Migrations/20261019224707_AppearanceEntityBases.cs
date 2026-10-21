using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppearanceEntityBases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "versao_tema",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "versao_tema",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "sessao_edicao_aparencia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "sessao_edicao_aparencia",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "revisao_aparencia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "revisao_aparencia",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "aparencia",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "aparencia",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE revisao_aparencia SET created_at = published_at;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "versao_tema");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "versao_tema");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "sessao_edicao_aparencia");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "sessao_edicao_aparencia");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "revisao_aparencia");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "revisao_aparencia");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "aparencia");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "aparencia");
        }
    }
}
