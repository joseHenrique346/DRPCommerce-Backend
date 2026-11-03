using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutAndOrderLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "pedido",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_locked",
                table: "pedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "payment_expires_at",
                table: "pedido",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "payment_method_id",
                table: "pedido",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "stock_reservation_id",
                table: "pedido",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "historico_status_pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    from_status_id = table.Column<long>(type: "bigint", nullable: true),
                    to_status_id = table.Column<long>(type: "bigint", nullable: false),
                    from_payment_status_id = table.Column<long>(type: "bigint", nullable: true),
                    to_payment_status_id = table.Column<long>(type: "bigint", nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actor_id = table.Column<long>(type: "bigint", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historico_status_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_historico_status_pedido_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historico_status_pedido_status_pagamento_pedido_from_paymen~",
                        column: x => x.from_payment_status_id,
                        principalTable: "status_pagamento_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historico_status_pedido_status_pagamento_pedido_to_payment_~",
                        column: x => x.to_payment_status_id,
                        principalTable: "status_pagamento_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historico_status_pedido_status_pedido_from_status_id",
                        column: x => x.from_status_id,
                        principalTable: "status_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historico_status_pedido_status_pedido_to_status_id",
                        column: x => x.to_status_id,
                        principalTable: "status_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pedido_enterprise_id_customer_id_idempotency_key",
                table: "pedido",
                columns: new[] { "enterprise_id", "customer_id", "idempotency_key" },
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_order_status_id_payment_expires_at",
                table: "pedido",
                columns: new[] { "order_status_id", "payment_expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_pedido_payment_method_id",
                table: "pedido",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_stock_reservation_id",
                table: "pedido",
                column: "stock_reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_historico_status_pedido_enterprise_id",
                table: "historico_status_pedido",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_historico_status_pedido_from_payment_status_id",
                table: "historico_status_pedido",
                column: "from_payment_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_historico_status_pedido_from_status_id",
                table: "historico_status_pedido",
                column: "from_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_historico_status_pedido_order_id_changed_at",
                table: "historico_status_pedido",
                columns: new[] { "order_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_historico_status_pedido_to_payment_status_id",
                table: "historico_status_pedido",
                column: "to_payment_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_historico_status_pedido_to_status_id",
                table: "historico_status_pedido",
                column: "to_status_id");

            migrationBuilder.AddForeignKey(
                name: "fk_pedido_metodo_transacao_payment_method_id",
                table: "pedido",
                column: "payment_method_id",
                principalTable: "metodo_transacao",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_pedido_reserva_estoque_stock_reservation_id",
                table: "pedido",
                column: "stock_reservation_id",
                principalTable: "reserva_estoque",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pedido_metodo_transacao_payment_method_id",
                table: "pedido");

            migrationBuilder.DropForeignKey(
                name: "fk_pedido_reserva_estoque_stock_reservation_id",
                table: "pedido");

            migrationBuilder.DropTable(
                name: "historico_status_pedido");

            migrationBuilder.DropIndex(
                name: "ix_pedido_enterprise_id_customer_id_idempotency_key",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "ix_pedido_order_status_id_payment_expires_at",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "ix_pedido_payment_method_id",
                table: "pedido");

            migrationBuilder.DropIndex(
                name: "ix_pedido_stock_reservation_id",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "is_locked",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "payment_expires_at",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "payment_method_id",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "stock_reservation_id",
                table: "pedido");
        }
    }
}
