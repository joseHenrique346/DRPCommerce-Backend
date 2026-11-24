using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecoverablePaymentOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operacao_pagamento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_transaction_id = table.Column<long>(type: "bigint", nullable: true),
                    payment_attempt_id = table.Column<long>(type: "bigint", nullable: true),
                    provider_connection_id = table.Column<long>(type: "bigint", nullable: true),
                    provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: true),
                    external_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    external_reference = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payment_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    preference_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    actor_id = table.Column<long>(type: "bigint", nullable: true),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    result_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    resolution_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    manual_confirmation_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    confirmed_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_reconciliation_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lease_token = table.Column<Guid>(type: "uuid", nullable: true),
                    lease_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operacao_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "fk_operacao_pagamento_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_operacao_pagamento_tentativa_pagamento_payment_attempt_id",
                        column: x => x.payment_attempt_id,
                        principalTable: "tentativa_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_operacao_pagamento_transacao_payment_transaction_id",
                        column: x => x.payment_transaction_id,
                        principalTable: "transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_operacao_pagamento_enterprise_id_order_id_kind",
                table: "operacao_pagamento",
                columns: new[] { "enterprise_id", "order_id", "kind" },
                unique: true,
                filter: "payment_transaction_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_operacao_pagamento_idempotency_key",
                table: "operacao_pagamento",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_operacao_pagamento_order_id",
                table: "operacao_pagamento",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_operacao_pagamento_payment_attempt_id",
                table: "operacao_pagamento",
                column: "payment_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_operacao_pagamento_payment_transaction_id_kind",
                table: "operacao_pagamento",
                columns: new[] { "payment_transaction_id", "kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_operacao_pagamento_status_next_reconciliation_at",
                table: "operacao_pagamento",
                columns: new[] { "status", "next_reconciliation_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM operacao_pagamento) THEN
                        RAISE EXCEPTION 'Há histórico de operações de pagamento. Use uma migração corretiva que preserve os registros.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropTable(
                name: "operacao_pagamento");
        }
    }
}
