using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReserveCouponUsesAtCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pedidos do checkout anteriores à correção também precisam ocupar uma utilização.
            // Preserva descontos já aceitos, inclusive se a versão anterior ultrapassou MaxUses.
            migrationBuilder.Sql("""
                LOCK TABLE pedido, cupom, resgate_cupom IN SHARE ROW EXCLUSIVE MODE;

                WITH new_redemptions AS (
                    INSERT INTO resgate_cupom (
                        enterprise_id, coupon_id, customer_id, order_id, discount_amount,
                        redeemed_at, released_at, created_at, updated_at
                    )
                    SELECT orders.enterprise_id, orders.coupon_id, orders.customer_id, orders.id,
                        orders.discount_amount, CURRENT_TIMESTAMP, NULL, CURRENT_TIMESTAMP, NULL
                    FROM pedido AS orders
                    INNER JOIN cupom AS coupon ON coupon.id = orders.coupon_id
                        AND coupon.enterprise_id = orders.enterprise_id
                    WHERE orders.is_deleted = FALSE
                        AND orders.is_locked = TRUE
                        AND orders.order_status_id = 1
                        AND orders.order_payment_status_id = 1
                        AND orders.discount_amount > 0
                        AND NOT EXISTS (
                            SELECT 1
                            FROM resgate_cupom AS redemption
                            WHERE redemption.order_id = orders.id AND redemption.released_at IS NULL
                        )
                    RETURNING enterprise_id, coupon_id
                ), new_uses AS (
                    SELECT enterprise_id, coupon_id, COUNT(*)::integer AS quantity
                    FROM new_redemptions
                    GROUP BY enterprise_id, coupon_id
                )
                UPDATE cupom AS coupon
                SET used_count = coupon.used_count + new_uses.quantity,
                    updated_at = CURRENT_TIMESTAMP
                FROM new_uses
                WHERE coupon.enterprise_id = new_uses.enterprise_id AND coupon.id = new_uses.coupon_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Preserva os compromissos e a auditoria: removê-los no downgrade poderia liberar usos de pedidos ainda abertos.
        }
    }
}
