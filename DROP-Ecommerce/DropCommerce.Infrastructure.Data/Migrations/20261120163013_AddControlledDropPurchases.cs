using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DropCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddControlledDropPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sessões antigas não representam admissão com CPF e prazo comercial de quinze minutos.
            // O comprador preserva o histórico e entra novamente pelo fluxo controlado.
            migrationBuilder.Sql("""
                UPDATE sessao_fila SET queue_session_status_id = 3
                WHERE queue_session_status_id = 1;
                """);

            migrationBuilder.DropIndex(
                name: "ix_pedido_drop_reservation_id",
                table: "pedido");

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "reserva",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purchase_identity_hash",
                table: "reserva",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "queue_session_id",
                table: "reserva",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "buyer_email_snapshot",
                table: "pedido",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "buyer_name_snapshot",
                table: "pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "financial_review_code",
                table: "pedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "fulfillment_acknowledged",
                table: "pedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payment_checkout_url",
                table: "pedido",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "payment_intent_accepted",
                table: "pedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "payment_operation_id",
                table: "pedido",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_resolution",
                table: "pedido",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "payment_revision",
                table: "pedido",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "provider_payment_id",
                table: "pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "item_name",
                table: "item_pedido",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<long>(
                name: "catalog_variant_id",
                table: "item_pedido",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "options_snapshot_json",
                table: "item_pedido",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purchase_identity_hash",
                table: "entrada_fila",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "linha_reserva_compra",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservation_id = table.Column<long>(type: "bigint", nullable: false),
                    variant_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    item_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    options_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_linha_reserva_compra", x => x.id);
                    table.ForeignKey(
                        name: "fk_linha_reserva_compra_catalogo_variante_variant_id",
                        column: x => x.variant_id,
                        principalTable: "catalogo_variante",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_linha_reserva_compra_reserva_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reserva",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reserva_queue_session_id_customer_id_idempotency_key",
                table: "reserva",
                columns: new[] { "queue_session_id", "customer_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pedido_drop_reservation_id",
                table: "pedido",
                column: "drop_reservation_id",
                unique: true,
                filter: "payment_operation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_payment_operation_id",
                table: "pedido",
                column: "payment_operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_catalog_variant_id",
                table: "item_pedido",
                column: "catalog_variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_fila_drop_event_id_purchase_identity_hash",
                table: "entrada_fila",
                columns: new[] { "drop_event_id", "purchase_identity_hash" });

            migrationBuilder.CreateIndex(
                name: "ix_linha_reserva_compra_reservation_id_variant_id",
                table: "linha_reserva_compra",
                columns: new[] { "reservation_id", "variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linha_reserva_compra_variant_id",
                table: "linha_reserva_compra",
                column: "variant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_item_pedido_catalogo_variante_catalog_variant_id",
                table: "item_pedido",
                column: "catalog_variant_id",
                principalTable: "catalogo_variante",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_reserva_sessao_fila_queue_session_id",
                table: "reserva",
                column: "queue_session_id",
                principalTable: "sessao_fila",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pedido WHERE payment_operation_id IS NOT NULL)
                        OR EXISTS (SELECT 1 FROM linha_reserva_compra) THEN
                        RAISE EXCEPTION 'Compras DROP controladas exigem migração de histórico antes de rollback.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_item_pedido_catalogo_variante_catalog_variant_id",
                table: "item_pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_reserva_sessao_fila_queue_session_id",
                table: "reserva");

            migrationBuilder.DropTable(
                name: "linha_reserva_compra");

            migrationBuilder.DropIndex(
                name: "ix_reserva_queue_session_id_customer_id_idempotency_key",
                table: "reserva");

            migrationBuilder.DropIndex(
                name: "ix_pedido_drop_reservation_id",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "ix_pedido_payment_operation_id",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "ix_item_pedido_catalog_variant_id",
                table: "item_pedido");

            migrationBuilder.DropIndex(
                name: "ix_entrada_fila_drop_event_id_purchase_identity_hash",
                table: "entrada_fila");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "reserva");

            migrationBuilder.DropColumn(
                name: "purchase_identity_hash",
                table: "reserva");

            migrationBuilder.DropColumn(
                name: "queue_session_id",
                table: "reserva");

            migrationBuilder.DropColumn(
                name: "buyer_email_snapshot",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "buyer_name_snapshot",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "financial_review_code",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "fulfillment_acknowledged",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "payment_checkout_url",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "payment_intent_accepted",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "payment_operation_id",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "payment_resolution",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "payment_revision",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "provider_payment_id",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "catalog_variant_id",
                table: "item_pedido");

            migrationBuilder.DropColumn(
                name: "options_snapshot_json",
                table: "item_pedido");

            migrationBuilder.DropColumn(
                name: "purchase_identity_hash",
                table: "entrada_fila");

            migrationBuilder.AlterColumn<string>(
                name: "item_name",
                table: "item_pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.CreateIndex(
                name: "ix_pedido_drop_reservation_id",
                table: "pedido",
                column: "drop_reservation_id");
        }
    }
}
