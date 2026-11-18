using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountErasureAndBuyerSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "personal_data_erased_at",
                table: "usuario",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "buyer_email",
                table: "pedido",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "buyer_name",
                table: "pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "buyer_phone",
                table: "pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "buyer_snapshot_captured_at",
                table: "pedido",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "buyer_snapshot_source",
                table: "pedido",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "state_id",
                table: "cliente",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "personal_data_erased_at",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: true);

            // Migração não conhece versões anteriores do perfil. Identifica a origem sem inventar
            // que o cadastro atual era exatamente o mesmo existente no instante da compra antiga.
            migrationBuilder.Sql("""
                UPDATE pedido AS p
                SET buyer_name = c.full_name,
                    buyer_email = c.email,
                    buyer_phone = c.phone,
                    buyer_snapshot_captured_at = CURRENT_TIMESTAMP,
                    buyer_snapshot_source = 2
                FROM cliente AS c
                WHERE p.customer_id = c.id AND p.enterprise_id = c.enterprise_id;

                UPDATE pedido
                SET buyer_name = 'Comprador indisponível',
                    buyer_snapshot_captured_at = CURRENT_TIMESTAMP,
                    buyer_snapshot_source = 3
                WHERE buyer_snapshot_captured_at IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A versão anterior exige campos pessoais não nulos. Reverter não pode fabricar dados apagados.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM cliente WHERE personal_data_erased_at IS NOT NULL)
                       OR EXISTS (SELECT 1 FROM usuario WHERE personal_data_erased_at IS NOT NULL) THEN
                        RAISE EXCEPTION 'Nao e possivel reverter a estrutura de privacidade apos apagar dados de contas.';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropColumn(
                name: "personal_data_erased_at",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "buyer_email",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "buyer_name",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "buyer_phone",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "buyer_snapshot_captured_at",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "buyer_snapshot_source",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "personal_data_erased_at",
                table: "cliente");

            migrationBuilder.AlterColumn<long>(
                name: "state_id",
                table: "cliente",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
