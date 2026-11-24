using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderConnectionsAndPaymentAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conexao_provedor",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    external_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    scopes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    protected_tokens = table.Column<string>(type: "character varying(64000)", maxLength: 64000, nullable: false),
                    access_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    refresh_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_connected = table.Column<bool>(type: "boolean", nullable: false),
                    connected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    disconnected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refreshed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    authorized_by_user_id = table.Column<long>(type: "bigint", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    refresh_lease_token = table.Column<Guid>(type: "uuid", nullable: true),
                    refresh_lease_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conexao_provedor", x => x.id);
                    table.UniqueConstraint("ak_conexao_provedor_id_enterprise_id_provider_environment_exte~", x => new { x.id, x.enterprise_id, x.provider, x.environment, x.external_account_id });
                    table.ForeignKey(
                        name: "fk_conexao_provedor_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_conexao_provedor_usuario_authorized_by_user_id",
                        column: x => x.authorized_by_user_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tentativa_pagamento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false),
                    provider_connection_id = table.Column<long>(type: "bigint", nullable: true),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    external_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    external_reference = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    transaction_method_id = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    preference_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    checkout_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    instructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    lease_token = table.Column<Guid>(type: "uuid", nullable: true),
                    lease_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_reconciliation_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tentativa_pagamento", x => x.id);
                    table.UniqueConstraint("ak_tentativa_pagamento_id_enterprise_id_provider_environment_e~", x => new { x.id, x.enterprise_id, x.provider, x.environment, x.external_account_id });
                    table.ForeignKey(
                        name: "fk_tentativa_pagamento_conexao_provedor_provider_connection_id~",
                        columns: x => new { x.provider_connection_id, x.enterprise_id, x.provider, x.environment, x.external_account_id },
                        principalTable: "conexao_provedor",
                        principalColumns: new[] { "id", "enterprise_id", "provider", "environment", "external_account_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tentativa_pagamento_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tentativa_pagamento_transacao_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pagamento_externo",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_attempt_id = table.Column<long>(type: "bigint", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    external_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payment_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamento_externo", x => x.id);
                    table.ForeignKey(
                        name: "fk_pagamento_externo_tentativa_pagamento_payment_attempt_id_en~",
                        columns: x => new { x.payment_attempt_id, x.enterprise_id, x.provider, x.environment, x.external_account_id },
                        principalTable: "tentativa_pagamento",
                        principalColumns: new[] { "id", "enterprise_id", "provider", "environment", "external_account_id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_conexao_provedor_authorized_by_user_id",
                table: "conexao_provedor",
                column: "authorized_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_conexao_provedor_enterprise_id_provider_environment",
                table: "conexao_provedor",
                columns: new[] { "enterprise_id", "provider", "environment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_externo_payment_attempt_id_enterprise_id_provider~",
                table: "pagamento_externo",
                columns: new[] { "payment_attempt_id", "enterprise_id", "provider", "environment", "external_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_externo_provider_environment_external_account_id_~",
                table: "pagamento_externo",
                columns: new[] { "provider", "environment", "external_account_id", "payment_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_enterprise_id_order_id",
                table: "tentativa_pagamento",
                columns: new[] { "enterprise_id", "order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_external_reference",
                table: "tentativa_pagamento",
                column: "external_reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_idempotency_key",
                table: "tentativa_pagamento",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_order_id",
                table: "tentativa_pagamento",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_provider_connection_id_enterprise_id_pr~",
                table: "tentativa_pagamento",
                columns: new[] { "provider_connection_id", "enterprise_id", "provider", "environment", "external_account_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_provider_environment_external_account_i~",
                table: "tentativa_pagamento",
                columns: new[] { "provider", "environment", "external_account_id", "preference_id" },
                unique: true,
                filter: "preference_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_status_next_reconciliation_at",
                table: "tentativa_pagamento",
                columns: new[] { "status", "next_reconciliation_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_transaction_id",
                table: "tentativa_pagamento",
                column: "transaction_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagamento_externo");

            migrationBuilder.DropTable(
                name: "tentativa_pagamento");

            migrationBuilder.DropTable(
                name: "conexao_provedor");
        }
    }
}
