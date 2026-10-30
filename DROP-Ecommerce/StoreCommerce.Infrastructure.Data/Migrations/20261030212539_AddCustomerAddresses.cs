using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "endereco_cliente",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_id = table.Column<long>(type: "bigint", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_endereco_cliente", x => x.id);
                    table.ForeignKey(
                        name: "fk_endereco_cliente_cliente_customer_id",
                        column: x => x.customer_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_endereco_cliente_estado_state_id",
                        column: x => x.state_id,
                        principalTable: "estado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // O endereço do cadastro de cada cliente ativo vira o endereço padrão salvo.
            migrationBuilder.Sql(@"
                INSERT INTO endereco_cliente (enterprise_id, customer_id, label, recipient_name, address_line, city, state_id, zip_code, country, is_default, created_at)
                SELECT enterprise_id, id, 'Cadastro', full_name, address_line, city, state_id, zip_code,
                       COALESCE(NULLIF(country, ''), 'Brasil'), TRUE, now()
                FROM cliente
                WHERE is_deleted = FALSE AND COALESCE(address_line, '') <> '' AND COALESCE(city, '') <> '' AND COALESCE(zip_code, '') <> '';");

            migrationBuilder.CreateIndex(
                name: "ix_endereco_cliente_customer_id_is_default",
                table: "endereco_cliente",
                columns: new[] { "customer_id", "is_default" });

            migrationBuilder.CreateIndex(
                name: "ix_endereco_cliente_enterprise_id",
                table: "endereco_cliente",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_endereco_cliente_state_id",
                table: "endereco_cliente",
                column: "state_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "endereco_cliente");
        }
    }
}
