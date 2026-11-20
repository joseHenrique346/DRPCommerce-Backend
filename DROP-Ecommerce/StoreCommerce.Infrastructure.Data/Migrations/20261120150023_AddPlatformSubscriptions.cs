using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assinatura_plataforma",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    offer_id = table.Column<long>(type: "bigint", nullable: false),
                    reference = table.Column<string>(type: "character varying(41)", maxLength: 41, nullable: false),
                    account_id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    remote_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    checkout_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remote_status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    billing_anchor = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    applied_contract_revision = table.Column<long>(type: "bigint", nullable: false),
                    pending_offer_id = table.Column<long>(type: "bigint", nullable: true),
                    change_effective_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_upgrade = table.Column<bool>(type: "boolean", nullable: false),
                    change_synchronized = table.Column<bool>(type: "boolean", nullable: false),
                    retained_membership_ids_json = table.Column<string>(type: "jsonb", nullable: true),
                    review_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    next_check_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assinatura_plataforma", x => x.id);
                    table.ForeignKey(
                        name: "fk_assinatura_plataforma_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assinatura_plataforma_solicitacao_contrato_empresa_offer_id",
                        column: x => x.offer_id,
                        principalTable: "solicitacao_contrato_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assinatura_plataforma_solicitacao_contrato_empresa_pending_~",
                        column: x => x.pending_offer_id,
                        principalTable: "solicitacao_contrato_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notificacao_cobranca_plataforma",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    delivery = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    topic = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    resource_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    next_check_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacao_cobranca_plataforma", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fatura_plataforma",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    subscription_id = table.Column<long>(type: "bigint", nullable: false),
                    offer_id = table.Column<long>(type: "bigint", nullable: false),
                    origin = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    is_upgrade = table.Column<bool>(type: "boolean", nullable: false),
                    reference = table.Column<string>(type: "character varying(41)", maxLength: 41, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    monthly_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checkout_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preference_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    checkout_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    payment_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    payment_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    applied_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fatura_plataforma", x => x.id);
                    table.ForeignKey(
                        name: "fk_fatura_plataforma_assinatura_plataforma_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "assinatura_plataforma",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fatura_plataforma_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fatura_plataforma_solicitacao_contrato_empresa_offer_id",
                        column: x => x.offer_id,
                        principalTable: "solicitacao_contrato_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assinatura_plataforma_account_id_environment_remote_id",
                table: "assinatura_plataforma",
                columns: new[] { "account_id", "environment", "remote_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assinatura_plataforma_enterprise_id",
                table: "assinatura_plataforma",
                column: "enterprise_id",
                unique: true,
                filter: "closed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_assinatura_plataforma_offer_id",
                table: "assinatura_plataforma",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_assinatura_plataforma_pending_offer_id",
                table: "assinatura_plataforma",
                column: "pending_offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_assinatura_plataforma_reference",
                table: "assinatura_plataforma",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fatura_plataforma_enterprise_id",
                table: "fatura_plataforma",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_plataforma_offer_id",
                table: "fatura_plataforma",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_fatura_plataforma_payment_id",
                table: "fatura_plataforma",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fatura_plataforma_reference",
                table: "fatura_plataforma",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fatura_plataforma_subscription_id_origin",
                table: "fatura_plataforma",
                columns: new[] { "subscription_id", "origin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_cobranca_plataforma_account_id_environment_topi~",
                table: "notificacao_cobranca_plataforma",
                columns: new[] { "account_id", "environment", "topic", "delivery" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_cobranca_plataforma_completed_at_next_check_at",
                table: "notificacao_cobranca_plataforma",
                columns: new[] { "completed_at", "next_check_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fatura_plataforma");

            migrationBuilder.DropTable(
                name: "notificacao_cobranca_plataforma");

            migrationBuilder.DropTable(
                name: "assinatura_plataforma");
        }
    }
}
