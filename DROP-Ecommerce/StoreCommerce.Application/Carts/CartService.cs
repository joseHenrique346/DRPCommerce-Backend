using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using CartEntity = StoreCommerce.Domain.Entity.Cart;

namespace StoreCommerce.Application.Carts;

/// <summary>
/// Casos de uso do carrinho do cliente autenticado na vitrine. O carrinho não reserva estoque: a disponibilidade
/// de cada item é calculada a cada consulta com o saldo atual do SKU.
/// </summary>
public sealed class CartService(
    ICartRepository carts,
    IProductRepository products,
    IRepository<Customer> customers,
    ICouponRepository coupons,
    CouponEligibilityService eligibility,
    StoreSettingsReader storeSettings,
    CategoryCatalogReader categoryReader,
    CartWriteScope writes,
    IUnitOfWork unitOfWork,
    ICurrentIdentity actor,
    ITenantProvider tenantProvider,
    TimeProvider time)
{
    private sealed record CartContext(long EnterpriseId, long CustomerId, StoreSettings Settings, IReadOnlySet<long> VisibleCategories);

    public async Task<CartResponse> GetAsync(string? couponCode, CancellationToken ct)
    {
        var context = await ReadContextAsync(ct);
        var cart = await carts.GetForCustomerAsync(context.EnterpriseId, context.CustomerId, tracking: false, ct);

        return await BuildResponseAsync(cart, context, couponCode, ct);
    }

    public async Task<CartResponse> AddItemAsync(long productId, long? productVariantId, int quantity, CancellationToken ct)
    {
        var context = await ReadContextAsync(ct);
        var product = (await products.GetWithVariantsAsync([productId], ct)).SingleOrDefault();

        if (product is null || !product.IsActive || !context.VisibleCategories.Contains(product.CategoryId))
            throw new SecurityFailure(404, "Product.NotFound", "Produto não encontrado.");

        var variant = productVariantId.HasValue
            ? product.ListProductVariant.SingleOrDefault(variant => variant.Id == productVariantId)
            : product.ListProductVariant.SingleOrDefault(variant => variant.IsDefault && !product.HasVariants);

        if (variant is null)
            throw new SecurityFailure(400, "ProductVariant.Required", product.HasVariants
                ? "Selecione uma variante do produto."
                : "SKU não pertence ao produto.");

        return await WriteAsync(context, createIfMissing: true, cart =>
            cart.AddItem(product, variant, quantity, context.Settings.AllowBackorder), ct);
    }

    public async Task<CartResponse> UpdateQuantityAsync(long itemId, int quantity, CancellationToken ct)
    {
        var context = await ReadContextAsync(ct);

        return await WriteAsync(context, createIfMissing: false, cart =>
        {
            EnsureItem(cart, itemId);
            cart.UpdateItemQuantity(itemId, quantity, context.Settings.AllowBackorder);
        }, ct);
    }

    public async Task<CartResponse> RemoveItemsAsync(IReadOnlyCollection<long> itemIds, CancellationToken ct)
    {
        var context = await ReadContextAsync(ct);

        return await WriteAsync(context, createIfMissing: false, cart =>
        {
            foreach (var itemId in itemIds.Distinct())
                EnsureItem(cart, itemId);

            foreach (var itemId in itemIds.Distinct())
                carts.RemoveItem(cart.RemoveItem(itemId));
        }, ct);
    }

    public async Task<CartResponse> ClearAsync(CancellationToken ct)
    {
        var context = await ReadContextAsync(ct);

        return await WriteAsync(context, createIfMissing: false, cart =>
        {
            foreach (var item in cart.Clear())
                carts.RemoveItem(item);
        }, ct);
    }

    public async Task<CartResponse> SetSelectionAsync(IReadOnlyCollection<long>? itemIds, bool selected, CancellationToken ct)
    {
        var context = await ReadContextAsync(ct);

        return await WriteAsync(context, createIfMissing: false, cart =>
        {
            if (itemIds is null || itemIds.Count == 0)
            {
                if (selected)
                    cart.SelectAllAvailable(context.Settings.AllowBackorder, context.VisibleCategories);
                else
                    cart.DeselectAll();

                return;
            }

            foreach (var itemId in itemIds)
                EnsureItem(cart, itemId);

            cart.SetSelection(itemIds, selected, context.Settings.AllowBackorder, context.VisibleCategories);
        }, ct);
    }

    public async Task<CartResponse> AwaitStockAsync(long itemId, CancellationToken ct)
    {
        var context = await ReadContextAsync(ct);

        return await WriteAsync(context, createIfMissing: false, cart =>
        {
            EnsureItem(cart, itemId);
            cart.AwaitStock(itemId, context.Settings.AllowBackorder, context.VisibleCategories);
        }, ct);
    }

    private async Task<CartContext> ReadContextAsync(CancellationToken ct)
    {
        var customerId = actor.CustomerId;
        var enterpriseId = tenantProvider.GetEnterpriseId();
        var categories = await categoryReader.ReadAsync(storefront: true, ct);

        if (categories.IsFailure)
            throw new SecurityFailure(404, "Store.Unavailable", "Loja indisponível.");

        if (!await customers.ExistsAsync(customer => customer.Id == customerId && customer.IsActive, ct))
            throw new SecurityFailure(401, "Identity.InvalidCredentials", "Credenciais inválidas ou acesso temporariamente indisponível.");

        return new CartContext(enterpriseId, customerId, await storeSettings.GetCurrentAsync(ct),
            categories.Content!.Select(category => category.Id).ToHashSet());
    }

    private async Task<CartResponse> WriteAsync(CartContext context, bool createIfMissing, Action<CartEntity> change, CancellationToken ct)
    {
        var cart = await carts.GetForCustomerAsync(context.EnterpriseId, context.CustomerId, tracking: true, ct);

        if (cart is null)
        {
            if (!createIfMissing)
                throw new SecurityFailure(404, "CartItem.NotFound", "Item não encontrado no carrinho.");

            cart = CartEntity.Create(context.EnterpriseId, context.CustomerId);
            carts.Add(cart);
        }

        using (writes.Begin(context.EnterpriseId, context.CustomerId))
        {
            change(cart);
            await unitOfWork.CommitAsync(ct);
        }

        return await BuildResponseAsync(cart, context, null, ct);
    }

    private static void EnsureItem(CartEntity cart, long itemId)
    {
        if (!cart.ContainsItem(itemId))
            throw new SecurityFailure(404, "CartItem.NotFound", $"Item {itemId} não encontrado no carrinho.");
    }

    private async Task<CartResponse> BuildResponseAsync(CartEntity? cart, CartContext context, string? couponCode, CancellationToken ct)
    {
        var settings = context.Settings;
        var items = (cart?.ListCartItem ?? [])
            .OrderByDescending(item => item.Id)
            .Select(item =>
            {
                var availability = item.GetAvailability(settings.AllowBackorder, context.VisibleCategories);

                return new CartItemResponse(
                    item.Id, item.ProductId, item.ProductVariantId, item.Product.Name, item.Product.Slug,
                    item.ProductVariant.Name, item.ProductVariant.AttributesJson, item.ProductVariant.SKU, item.Product.ImageUrls,
                    item.UnitPrice, item.Quantity, item.LineTotal, item.ProductVariant.AvailableQuantity, !item.Product.IsDigital,
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

    private async Task<CartCouponPreview> PreviewCouponAsync(string couponCode, decimal subTotal, int quantity, CartContext context, CancellationToken ct)
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
