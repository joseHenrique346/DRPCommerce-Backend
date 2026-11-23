using CartEntity = StoreCommerce.Domain.Entity.Cart;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Interfaces.Orders;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CartResponseBuilder(
    ICouponRepository coupons,
    CouponCustomerHistoryReader eligibility,
    TimeProvider time)
{
    public async Task<CartResponse> BuildAsync(CartEntity? cart, CartPurchaseContext context, string? couponCode, CancellationToken ct)
    {
        var settings = context.Settings;
        var items = (cart?.ListCartItem ?? [])
            .OrderByDescending(item => item.Id)
            .Select(item =>
            {
                var availability = item.GetAvailability(settings.AllowBackorder, context.VisibleCategories);
                var hidden = item.Product.IsContractBlocked;

                return new CartItemResponse(
                    item.Id, item.ProductId, item.ProductVariantId, hidden ? "Produto indisponível" : item.Product.Name,
                    hidden ? string.Empty : item.Product.Slug,
                    hidden ? string.Empty : item.ProductVariant.Name, hidden ? "{}" : item.ProductVariant.AttributesJson,
                    hidden ? string.Empty : item.ProductVariant.SKU, hidden ? string.Empty : item.Product.ImageUrls,
                    hidden ? 0m : item.UnitPrice, item.Quantity, hidden ? 0m : item.LineTotal,
                    hidden ? 0 : item.ProductVariant.AvailableQuantity, !item.Product.IsDigital,
                    availability, item.IsSelected, item.IsAwaitingStock,
                    RequiresDecision: availability != CartItemAvailability.Available && !item.IsAwaitingStock,
                    BackInStock: availability == CartItemAvailability.Available && item.IsAwaitingStock);
            })
            .ToList();

        var purchasable = items.Where(item => item.IsSelected && item.Availability == CartItemAvailability.Available).ToList();
        var subTotal = purchasable.Sum(item => item.LineTotal);
        var quantity = purchasable.Sum(item => item.Quantity);
        var hasBlocked = items.Any(item => item.IsSelected && item.Availability != CartItemAvailability.Available);
        var preview = string.IsNullOrWhiteSpace(couponCode) ? null : await PreviewCouponAsync(couponCode, subTotal, quantity, context, ct);
        var discount = preview?.IsValid == true ? preview.DiscountAmount : 0;
        var meetsMinimum = settings.MeetsMinOrderValue(subTotal);
        var withinLimit = settings.IsWithinItemLimit(quantity);

        return new CartResponse(
            cart is null || cart.Id == 0 ? null : cart.Id,
            settings.CurrencyCode,
            items,
            items.Count,
            items.Count(item => item.RequiresDecision),
            new CartSummaryResponse(
                purchasable.Count, quantity, subTotal, discount, subTotal - discount,
                settings.MinOrderValue, meetsMinimum, settings.MaxItemsPerOrder, withinLimit, hasBlocked,
                CanCheckout: purchasable.Count > 0 && !hasBlocked && meetsMinimum && withinLimit,
                preview));
    }

    private async Task<CartCouponPreview> PreviewCouponAsync(string couponCode, decimal subTotal, int quantity, CartPurchaseContext context, CancellationToken ct)
    {
        var code = Coupon.NormalizeCode(couponCode);
        var coupon = await coupons.GetByCodeAsync(code, ct);

        if (coupon is null)
            return new CartCouponPreview(code, false, 0, "Coupon.NotFound", "Cupom não encontrado.");

        var history = await eligibility.GetHistoryAsync(coupon.Id, context.CustomerId, null, ct);
        var evaluation = coupon.Evaluate(time.GetUtcNow().UtcDateTime, subTotal, quantity, history);

        return new CartCouponPreview(coupon.Code, evaluation.IsValid, evaluation.DiscountAmount, evaluation.ErrorCode, evaluation.Message);
    }
}
