using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMercadoPagoOAuthAndNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "next_status_check_at",
                table: "tentativa_pagamento",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "captured_transaction_id",
                table: "pagamento_externo",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "provider_updated_at",
                table: "pagamento_externo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "autorizacao_provedor",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    membership_id = table.Column<long>(type: "bigint", nullable: false),
                    user_version = table.Column<long>(type: "bigint", nullable: false),
                    state_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    browser_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    protected_verifier = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_autorizacao_provedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_autorizacao_provedor_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_autorizacao_provedor_usuario_user_id",
                        column: x => x.user_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacao_provedor",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    provider_connection_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_attempt_id = table.Column<long>(type: "bigint", nullable: true),
                    payment_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    delivery_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    resolution_code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    lease_token = table.Column<Guid>(type: "uuid", nullable: true),
                    lease_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacao_provedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_notificacao_provedor_conexao_provedor_provider_connection_id",
                        column: x => x.provider_connection_id,
                        principalTable: "conexao_provedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacao_provedor_tentativa_pagamento_payment_attempt_id",
                        column: x => x.payment_attempt_id,
                        principalTable: "tentativa_pagamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "selecao_provedor_pagamento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_selecao_provedor_pagamento", x => x.id);
                    table.ForeignKey(
                        name: "fk_selecao_provedor_pagamento_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tentativa_pagamento_provider_status_next_status_check_at",
                table: "tentativa_pagamento",
                columns: new[] { "provider", "status", "next_status_check_at" });

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_externo_captured_transaction_id",
                table: "pagamento_externo",
                column: "captured_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_autorizacao_provedor_enterprise_id",
                table: "autorizacao_provedor",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_autorizacao_provedor_expires_at",
                table: "autorizacao_provedor",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_autorizacao_provedor_state_hash",
                table: "autorizacao_provedor",
                column: "state_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_autorizacao_provedor_user_id",
                table: "autorizacao_provedor",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_provedor_payment_attempt_id",
                table: "notificacao_provedor",
                column: "payment_attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_provedor_provider_connection_id_delivery_hash",
                table: "notificacao_provedor",
                columns: new[] { "provider_connection_id", "delivery_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_provedor_status_next_attempt_at",
                table: "notificacao_provedor",
                columns: new[] { "status", "next_attempt_at" });

            migrationBuilder.CreateIndex(
                name: "ix_selecao_provedor_pagamento_enterprise_id",
                table: "selecao_provedor_pagamento",
                column: "enterprise_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_pagamento_externo_transacao_captured_transaction_id",
                table: "pagamento_externo",
                column: "captured_transaction_id",
                principalTable: "transacao",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM notificacao_provedor)
                        OR EXISTS (SELECT 1 FROM selecao_provedor_pagamento WHERE provider <> 'manual')
                        OR EXISTS (SELECT 1 FROM pagamento_externo WHERE captured_transaction_id IS NOT NULL) THEN
                        RAISE EXCEPTION 'Há integração financeira ou histórico de notificações. Use migração corretiva que preserve os registros.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_pagamento_externo_transacao_captured_transaction_id",
                table: "pagamento_externo");

            migrationBuilder.DropTable(
                name: "autorizacao_provedor");

            migrationBuilder.DropTable(
                name: "notificacao_provedor");

            migrationBuilder.DropTable(
                name: "selecao_provedor_pagamento");

            migrationBuilder.DropIndex(
                name: "ix_tentativa_pagamento_provider_status_next_status_check_at",
                table: "tentativa_pagamento");

            migrationBuilder.DropIndex(
                name: "ix_pagamento_externo_captured_transaction_id",
                table: "pagamento_externo");

            migrationBuilder.DropColumn(
                name: "next_status_check_at",
                table: "tentativa_pagamento");

            migrationBuilder.DropColumn(
                name: "captured_transaction_id",
                table: "pagamento_externo");

            migrationBuilder.DropColumn(
                name: "provider_updated_at",
                table: "pagamento_externo");
        }
    }
}
