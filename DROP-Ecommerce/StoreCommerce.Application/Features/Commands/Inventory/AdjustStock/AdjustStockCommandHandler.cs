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

public sealed class AdjustStockCommandHandler(
    IInventoryRepository repository,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    ITenantProvider tenant,
    InventoryWriteScope writes,
    ICurrentIdentity actor,
    TimeProvider time,
    StoreSettingsReader settings,
    StockReservationExpirationWriter expiration)
    : CatalogRequestHandler<AdjustStockCommand, StockMovementResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<StockMovementResponse> ExecuteAsync(AdjustStockCommand command, CancellationToken ct)
    {
        var variantId = command.ProductVariantId;
        var request = command.Request;
        var enterpriseId = tenant.GetEnterpriseId();
        var userId = actor.UserId;
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        await expiration.ExpireAsync(enterpriseId, Now, ct);
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
        TrackedStockPolicy.EnsureTracked(variant);

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
}
