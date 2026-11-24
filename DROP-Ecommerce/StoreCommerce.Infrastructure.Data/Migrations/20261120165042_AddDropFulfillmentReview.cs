using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDropFulfillmentReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "fulfillment_review_required",
                table: "pagamento_venda_drop",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fulfillment_review_required",
                table: "pagamento_venda_drop");
        }
    }
}
