using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDropSalePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pagamento_venda_drop",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drop_order_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    provider = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    environment = table.Column<int>(type: "integer", nullable: false),
                    external_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    provider_connection_id = table.Column<long>(type: "bigint", nullable: true),
                    external_reference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    preference_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    checkout_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    payment_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    acknowledged_revision = table.Column<long>(type: "bigint", nullable: false),
                    next_check_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fulfillment_closed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pagamento_venda_drop", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_venda_drop_enterprise_id_drop_order_id",
                table: "pagamento_venda_drop",
                columns: new[] { "enterprise_id", "drop_order_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_venda_drop_enterprise_id_operation_id",
                table: "pagamento_venda_drop",
                columns: new[] { "enterprise_id", "operation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pagamento_venda_drop_external_reference",
                table: "pagamento_venda_drop",
                column: "external_reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pagamento_venda_drop");
        }
    }
}
