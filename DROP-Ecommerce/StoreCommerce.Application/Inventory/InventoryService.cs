using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Inventory;

public sealed class InventoryService(
    IInventoryRepository repository, IProductRepository products, IUnitOfWork unitOfWork,
    ITenantProvider tenant, InventoryWriteScope writes, ICurrentIdentity actor, TimeProvider time,
    StoreSettingsReader settings, CategoryCatalogReader categories)
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    private async Task<IStoreTransaction> BeginAsync(long enterpriseId, CancellationToken ct)
    {
        var transaction = await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // Mesmo bloqueio do catálogo: reservas não concorrem com exclusão ou conversão dos SKUs.
            await products.LockCatalogAsync(enterpriseId, ct);

            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();

            throw;
        }
    }

    public async Task<StockMovementResponse> AdjustAsync(long variantId, AdjustStockRequest request, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var userId = actor.UserId;
        await using var transaction = await BeginAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        await ExpireCoreAsync(enterpriseId, Now, ct);
        var key = request.IdempotencyKey.Trim();
        var previous = await repository.GetAdjustmentAsync(enterpriseId, variantId, key, ct);

        if (previous is not null)
        {
            if (previous.QuantityChange != request.QuantityChange || previous.Reason != request.Reason.Trim())
                throw new SecurityFailure(409, "Inventory.IdempotencyConflict", "A chave já foi usada em outro ajuste deste SKU.");

            await unitOfWork.CommitAsync(ct);
            await transaction.CommitAsync(ct);

            return StockMovementResponse.FromEntity(previous);
        }

        var variant = (await repository.GetVariantsAsync(enterpriseId, [variantId], ct)).SingleOrDefault()
            ?? throw new SecurityFailure(404, "ProductVariant.NotFound", "SKU não encontrado na loja atual.");
        EnsureTracked(variant);

        if (request.ExpectedVersion.HasValue && request.ExpectedVersion != variant.Version)
            throw new SecurityFailure(409, "Inventory.StaleVersion", "O saldo foi alterado. Atualize a consulta antes de ajustar.");

        var policy = await settings.GetCurrentAsync(ct);
        var next = (long)variant.StockQuantity + request.QuantityChange;

        if (next < int.MinValue || next > int.MaxValue ||
            (request.QuantityChange < 0 && !policy.AllowBackorder && (next < 0 || next < variant.ReservedQuantity)))
            throw new SecurityFailure(409, "Inventory.InvalidAdjustment", "O ajuste excede o limite do saldo ou compromete unidades reservadas.");

        variant.AdjustStock(request.QuantityChange, policy.AllowBackorder);
        var movement = StockMovement.Create(variant, null, StockMovementType.Adjustment, key,
            request.QuantityChange, 0, request.Reason.Trim(), userId, Now);
        repository.AddMovement(movement);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return StockMovementResponse.FromEntity(movement);
    }

    public async Task<StockReservationResponse> ReserveAsync(ReserveStockRequest request, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var userId = actor.UserId;
        await using var transaction = await BeginAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        var now = Now;
        await ExpireCoreAsync(enterpriseId, now, ct);
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

            EnsureTracked(variant);

            if (!variant.IsActive || !variant.Product.IsActive || !visibleIds.Contains(variant.Product.CategoryId))
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

    public async Task<StockReservationResponse> CompleteAsync(long id, bool commit, CancellationToken ct)
    {
        var enterpriseId = tenant.GetEnterpriseId();
        var userId = actor.UserId;
        await using var transaction = await BeginAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        var now = Now;
        await ExpireCoreAsync(enterpriseId, now, ct);
        var reservation = await repository.GetReservationAsync(enterpriseId, id, ct)
            ?? throw new SecurityFailure(404, "Inventory.ReservationNotFound", "Reserva não encontrada na loja atual.");
        var expected = commit ? StockReservationStatus.Committed : StockReservationStatus.Released;

        var repeated = reservation.Status == expected || (!commit && reservation.Status == StockReservationStatus.Expired);

        if (reservation.Status != StockReservationStatus.Reserved && !repeated)
            throw new SecurityFailure(409, "Inventory.ReservationClosed", "A reserva já foi encerrada por outra operação.");

        if (reservation.Status == StockReservationStatus.Reserved)
        {
            foreach (var line in reservation.Lines)
            {
                var variant = line.ProductVariant;

                if (commit && (long)variant.StockQuantity - line.Quantity < int.MinValue)
                    throw new SecurityFailure(409, "Inventory.QuantityOverflow", "O SKU atingiu o limite de saldo negativo.");

                if (commit)
                    variant.CommitStock(line.Quantity);
                else
                    variant.ReleaseStock(line.Quantity);

                repository.AddMovement(StockMovement.Create(variant, reservation, commit ? StockMovementType.Commit : StockMovementType.Release,
                    reservation.IdempotencyKey, commit ? -line.Quantity : 0, -line.Quantity,
                    commit ? "Baixa de reserva" : "Liberação de reserva", userId, now));
            }

            reservation.Complete(expected, now);
        }

        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return StockReservationResponse.FromEntity(reservation);
    }

    public async Task ExpireAsync(long enterpriseId, CancellationToken ct)
    {
        await using var transaction = await BeginAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        await ExpireCoreAsync(enterpriseId, Now, ct);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private async Task ExpireCoreAsync(long enterpriseId, DateTime now, CancellationToken ct)
    {
        var expired = await repository.GetExpiredAsync(enterpriseId, now, ct);

        foreach (var reservation in expired)
        {
            foreach (var line in reservation.Lines)
            {
                var variant = line.ProductVariant;
                variant.ReleaseStock(line.Quantity);
                repository.AddMovement(StockMovement.Create(variant, reservation, StockMovementType.Expiration,
                    reservation.IdempotencyKey, 0, -line.Quantity, "Expiração de reserva", null, now));
            }

            reservation.Complete(StockReservationStatus.Expired, now);
        }
    }

    private static void EnsureTracked(ProductVariant variant)
    {
        if (variant.Product.IsDigital || (variant.Product.HasVariants && variant.IsDefault))
            throw new SecurityFailure(400, "Inventory.UntrackedSku", "O SKU padrão de um produto com variantes e os produtos digitais não controlam estoque.");
    }
}
