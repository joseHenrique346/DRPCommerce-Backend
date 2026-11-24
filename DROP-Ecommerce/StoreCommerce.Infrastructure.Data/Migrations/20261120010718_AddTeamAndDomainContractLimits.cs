using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamAndDomainContractLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "retained_membership_ids_json",
                table: "solicitacao_contrato_empresa",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "contract_blocked_at",
                table: "membro_empresa",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_contract_blocked",
                table: "membro_empresa",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_contract_blocked",
                table: "dominio_empresa",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "retained_membership_ids_json",
                table: "solicitacao_contrato_empresa");

            migrationBuilder.DropColumn(
                name: "contract_blocked_at",
                table: "membro_empresa");

            migrationBuilder.DropColumn(
                name: "is_contract_blocked",
                table: "membro_empresa");

            migrationBuilder.DropColumn(
                name: "is_contract_blocked",
                table: "dominio_empresa");
        }
    }
}
