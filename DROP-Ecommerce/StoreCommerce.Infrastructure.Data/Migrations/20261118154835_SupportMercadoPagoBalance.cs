using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SupportMercadoPagoBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "metodo_transacao",
                columns: new[] { "id", "description" },
                values: new object[] { 4L, "Saldo Mercado Pago" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "metodo_transacao",
                keyColumn: "id",
                keyValue: 4L);
        }
    }
}
