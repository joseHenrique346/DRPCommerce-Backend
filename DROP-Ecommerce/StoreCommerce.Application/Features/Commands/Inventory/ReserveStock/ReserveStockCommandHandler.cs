using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Inventory;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Inventory;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReserveStockCommandHandler(
    IInventoryRepository repository,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    ITenantProvider tenant,
    InventoryWriteScope writes,
    ICurrentIdentity actor,
    TimeProvider time,
    StoreSettingsReader settings,
    CategoryCatalogReader categories,
    StockReservationExpirationWriter expiration)
    : CatalogRequestHandler<ReserveStockCommand, StockReservationResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<StockReservationResponse> ExecuteAsync(ReserveStockCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var enterpriseId = tenant.GetEnterpriseId();
        var userId = actor.UserId;
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        var now = Now;
        await expiration.ExpireAsync(enterpriseId, now, ct);
        var key = request.IdempotencyKey.Trim();
        var previous = await repository.FindByKeyAsync(enterpriseId, key, ct);
        var items = request.Items.OrderBy(Item => Item.ProductVariantId).ToList();

        if (previous is not null)
        {
            var previousItems = previous.Lines.OrderBy(Line => Line.ProductVariantId).Select(Line => (Line.ProductVariantId, Line.Quantity));

            if (previous.Reference != (request.Reference?.Trim() ?? string.Empty) ||
                !previousItems.SequenceEqual(items.Select(Item => (Item.ProductVariantId, Item.Quantity))))
                throw new SecurityFailure(409, "Inventory.IdempotencyConflict", "A chave já foi usada em outra reserva.");

            await unitOfWork.CommitAsync(ct);
            await transaction.CommitAsync(ct);

            return StockReservationResponse.FromEntity(previous);
        }

        var visible = await categories.ReadAsync(true, ct);

        if (visible.IsFailure)
            throw new SecurityFailure(409, "Store.Inactive", "A loja está indisponível para novas reservas.");

        var visibleIds = visible.Content!.Select(Category => Category.Id).ToHashSet();
        var policy = await settings.GetCurrentAsync(ct);
        var total = items.Sum(Item => (long)Item.Quantity);

        if (total > 10000 || (policy.MaxItemsPerOrder.HasValue && total > policy.MaxItemsPerOrder))
            throw new SecurityFailure(400, "Inventory.ItemLimit", "A reserva excede o limite de unidades permitido por pedido.");

        var variants = (await repository.GetVariantsAsync(enterpriseId, items.Select(Item => Item.ProductVariantId).ToList(), ct))
            .ToDictionary(Variant => Variant.Id);

        foreach (var item in items)
        {
            if (!variants.TryGetValue(item.ProductVariantId, out var variant))
                throw new SecurityFailure(404, "ProductVariant.NotFound", "Um dos SKUs não foi encontrado na loja atual.");

            TrackedStockPolicy.EnsureTracked(variant);

            if (!variant.IsActive || !variant.Product.IsActive || variant.Product.IsContractBlocked || !visibleIds.Contains(variant.Product.CategoryId))
                throw new SecurityFailure(409, "Inventory.SkuUnavailable", "Um dos SKUs ou sua categoria está indisponível.");

            if (!policy.AllowBackorder && variant.AvailableQuantity < item.Quantity)
                throw new SecurityFailure(409, "Inventory.InsufficientStock", $"Estoque insuficiente para o SKU {variant.SKU}.");

            if ((long)variant.ReservedQuantity + item.Quantity > int.MaxValue)
                throw new SecurityFailure(409, "Inventory.QuantityOverflow", "O SKU atingiu o limite de unidades reservadas.");
        }

        var reservation = StockReservation.Create(enterpriseId, key, request.Reference, now.AddMinutes(policy.PendingOrderExpirationMinutes));
        repository.AddReservation(reservation);

        foreach (var item in items)
        {
            var variant = variants[item.ProductVariantId];
            reservation.AddLine(variant, item.Quantity, policy.AllowBackorder);
            variant.ReserveStock(item.Quantity, policy.AllowBackorder);
            repository.AddMovement(StockMovement.Create(variant, reservation, StockMovementType.Reservation, key,
                0, item.Quantity, "Reserva de estoque", userId, now));
        }

        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return StockReservationResponse.FromEntity(reservation);
    }
}
