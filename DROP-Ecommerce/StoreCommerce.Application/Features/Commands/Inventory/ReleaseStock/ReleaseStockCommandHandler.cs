using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Contracts.Inventory;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Inventory;
using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Interfaces.Catalog;
using StoreCommerce.Domain.Interfaces.Inventory;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReleaseStockCommandHandler(
    IInventoryRepository repository,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    ITenantProvider tenant,
    InventoryWriteScope writes,
    ICurrentIdentity actor,
    TimeProvider time,
    StockReservationExpirationWriter expiration,
    StockReservationCompletionWriter completion)
    : CatalogRequestHandler<ReleaseStockCommand, StockReservationResponse>
{
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    protected override async Task<StockReservationResponse> ExecuteAsync(ReleaseStockCommand command, CancellationToken ct)
    {
        var id = command.Id;
        var commit = false;
        var enterpriseId = tenant.GetEnterpriseId();
        var userId = actor.UserId;
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await products.LockCatalogAsync(enterpriseId, ct);
        using var operation = writes.Begin(enterpriseId);
        var now = Now;
        var reservation = await repository.GetReservationAsync(enterpriseId, id, ct)
            ?? throw new SecurityFailure(404, "Inventory.ReservationNotFound", "Reserva não encontrada na loja atual.");

        if (await repository.IsAttachedToOrderAsync(enterpriseId, id, ct))
            throw new SecurityFailure(409, "Inventory.OrderReservationProtected",
                "A reserva pertence a um pedido. Use a confirmação de pagamento ou o cancelamento do pedido.");

        await expiration.ExpireAsync(enterpriseId, now, ct);
        var expected = commit ? StockReservationStatus.Committed : StockReservationStatus.Released;

        var repeated = reservation.Status == expected || (!commit && reservation.Status == StockReservationStatus.Expired);

        if (reservation.Status != StockReservationStatus.Reserved && !repeated)
            throw new SecurityFailure(409, "Inventory.ReservationClosed", "A reserva já foi encerrada por outra operação.");

        completion.Complete(reservation, commit, userId, now);

        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return StockReservationResponse.FromEntity(reservation);
    }
}
