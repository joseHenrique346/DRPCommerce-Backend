using DropCommerce.Application.Interfaces.Identity;
using DropCommerce.Application.Interfaces.Inventory;
using DropCommerce.Domain.Interfaces.Persistence;
using System.Text.Json;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReturnDropCatalogStockCommandHandler(
    IDropStockSagaRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor)
    : IRequestHandler<ReturnDropCatalogStockCommand, Result<DropStockReturnIntent>>
{
    public async Task<Result<DropStockReturnIntent>> Handle(ReturnDropCatalogStockCommand command, CancellationToken ct)
    {
        long productId = command.ProductId;

        if (!actor.IsAuthenticated || !actor.HasPermission("drop.events.manage"))
            throw new SecurityFailure(403, "DropStock.PermissionRequired", "Permissão de gestão DROP necessária.");

        var tenant = actor.EnterpriseId;
        await persistence.RefreshCommercialRightsAsync(tenant, ct);
        using var operation = writes.Begin("transfer", tenant);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var existing = await persistence.FindPendingReturnAsync(tenant, productId, ct);

        if (existing is not null)
            return Result<DropStockReturnIntent>.Success(existing);

        var product = await persistence.FindProductWithVariantsAsync(productId, ct)
            ?? throw new SecurityFailure(404, "DropStock.NotFound", "Produto não encontrado.");

        var future = await persistence.HasFutureEventsAsync(product.Id, tenant, ct);
        var reservations = await persistence.HasReservationsAsync(product.Id, tenant, ct);
        var orders = await persistence.HasPendingOrdersAsync(product.Id, tenant, ct);

        if (future || reservations || orders || product.Variants.Any(Variant => Variant.QuantityReserved != 0))
            throw new SecurityFailure(409, "DropStock.ActiveOperations", "Encerre os eventos e resolva reservas e pedidos antes do retorno.");

        if (!product.StoreProductId.HasValue || !product.SourceExportId.HasValue)
            throw new SecurityFailure(409, "DropStock.NoSource", "O produto não possui origem Store para retorno.");

        var variantIds = JsonSerializer.Deserialize<long[]>(product.SourceVariantIdsJson)!;
        var variants = product.Variants.Where(Variant => Variant.SourceVariantId.HasValue && variantIds.Contains(Variant.SourceVariantId.Value))
            .Select(Variant => new DropStockVariant(Variant.SourceVariantId!.Value, Variant.SKU, Variant.OptionsJson, Variant.Price, Variant.Withdraw())).ToList();
        product.StartReturn();
        var snapshot = new DropStockReturn(Guid.NewGuid(), product.SourceExportId.Value, tenant, product.StoreProductId.Value, variants);
        var intent = DropStockReturnIntent.Create(snapshot.OperationId, tenant, product.Id, JsonSerializer.Serialize(snapshot));
        persistence.Add(intent);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<DropStockReturnIntent>.Success(intent);
    }
}
