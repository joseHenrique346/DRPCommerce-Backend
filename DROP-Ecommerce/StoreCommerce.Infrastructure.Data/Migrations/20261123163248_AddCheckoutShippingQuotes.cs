using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutShippingQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "pickup_at_store",
                table: "pedido",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "shipping_quote",
                table: "pedido",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "allow_pickup",
                table: "configuracao_loja",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pickup_at_store",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "shipping_quote",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "allow_pickup",
                table: "configuracao_loja");
        }
    }
}
