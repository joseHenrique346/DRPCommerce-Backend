using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStorefrontCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carrinho",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carrinho", x => x.id);
                    table.ForeignKey(
                        name: "fk_carrinho_cliente_customer_id",
                        column: x => x.customer_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_carrinho_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_carrinho",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    cart_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    product_variant_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    is_selected = table.Column<bool>(type: "boolean", nullable: false),
                    is_awaiting_stock = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_carrinho", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_carrinho_carrinho_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carrinho",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_item_carrinho_produto_product_id",
                        column: x => x.product_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_item_carrinho_variante_produto_product_variant_id",
                        column: x => x.product_variant_id,
                        principalTable: "variante_produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_carrinho_customer_id",
                table: "carrinho",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_carrinho_enterprise_id_customer_id",
                table: "carrinho",
                columns: new[] { "enterprise_id", "customer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_carrinho_cart_id_product_variant_id",
                table: "item_carrinho",
                columns: new[] { "cart_id", "product_variant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_carrinho_enterprise_id",
                table: "item_carrinho",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_carrinho_product_id",
                table: "item_carrinho",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_carrinho_product_variant_id",
                table: "item_carrinho",
                column: "product_variant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_carrinho");

            migrationBuilder.DropTable(
                name: "carrinho");
        }
    }
}
