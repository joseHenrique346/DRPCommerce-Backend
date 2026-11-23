using StoreCommerce.Application.Inventory;
using MediatR;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ExpireStockReservationsCommandHandler(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    InventoryWriteScope writes,
    TimeProvider time,
    StockReservationExpirationWriter expiration)
    : IRequestHandler<ExpireStockReservationsCommand>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    public async Task Handle(ExpireStockReservationsCommand command, CancellationToken ct)
    {
        var enterpriseId = command.EnterpriseId;
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        await expiration.ExpireAsync(enterpriseId, Now, ct);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
