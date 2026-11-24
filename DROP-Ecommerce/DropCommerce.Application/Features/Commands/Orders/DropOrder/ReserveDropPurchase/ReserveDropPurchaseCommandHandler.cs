using DropCommerce.Application.Interfaces.Identity;
using DropCommerce.Application.Interfaces.Queue;
using DropCommerce.Domain.Interfaces.Persistence;
using System.Security.Cryptography;
using System.Text;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using DropCommerce.Application.Security;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReserveDropPurchaseCommandHandler(
    IDropQueueRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor,
    TimeProvider clock,
    IMediator mediator)
    : IRequestHandler<ReserveDropPurchaseCommand, Result<DropReservationResponse>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;
    private long Customer => actor.CustomerId;

    public async Task<Result<DropReservationResponse>> Handle(ReserveDropPurchaseCommand command, CancellationToken ct)
    {
        long eventId = command.EventId;
        ReserveDropPurchase request = command.Input;

        var tenant = actor.EnterpriseId;
        var customer = Customer;
        await mediator.SendRequiredAsync(new ExpireDropPurchasesCommand(tenant, eventId), ct);
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop)
            throw Failure("DropPurchase.InvalidReservation", "Contrato ou reserva inválida.");

        using var operation = writes.Begin("sales", tenant);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var session = await persistence.FindSessionAsync(request.SessionId, customer, eventId, tenant, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.SessionNotFound", "Sessão de compra não encontrada.");

        if (session.QueueSessionStatusId != 1 || Now >= session.ExpiresAt || session.QueueEntry.PurchaseIdentityHash is null ||
            Now >= session.QueueEntry.DropEvent.DropEndsAt)
            throw Failure("DropPurchase.SessionExpired", "O prazo de compra terminou.");

        var existing = await persistence.FindReservationAsync(session.Id, customer, request.IdempotencyKey, ct);

        if (existing is not null)
        {
            if (existing.DropProductId != request.ParticipationId || !existing.Lines.OrderBy(Line => Line.VariantId)
                .Select(Line => (Line.VariantId, Line.Quantity)).SequenceEqual(request.Lines.OrderBy(Line => Line.VariantId).Select(Line => (Line.VariantId, Line.Quantity))))
                throw Failure("DropPurchase.IdempotencyConflict", "A chave já identifica outra reserva.");

            return Result<DropReservationResponse>.Success(Capture(existing));
        }

        var participation = await persistence.FindParticipationAsync(request.ParticipationId, eventId, tenant, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.ProductUnavailable", "Produto indisponível.");
        var product = participation.CatalogProduct;

        if (product is null || product.IsDeleted || product.IsContractBlocked || product.IsReturning || product.IsReturned ||
            product.StoreProductId.HasValue && !product.SourceExportId.HasValue)
            throw Failure("DropPurchase.ProductUnavailable", "O produto não está disponível para reservas.");

        var variants = request.Lines.Select(Line => (Line, Variant: product.Variants.SingleOrDefault(Variant => Variant.Id == Line.VariantId))).ToList();

        if (variants.Any(Item => Item.Variant is null || Item.Line.Quantity > Item.Variant.QuantityOnHand - Item.Variant.QuantityReserved))
            throw Failure("DropPurchase.InsufficientStock", "Há variações indisponíveis ou sem saldo suficiente.");

        var quantity = request.Lines.Sum(Line => (long)Line.Quantity);
        var used = await persistence.ReservedUnitsAsync(eventId, product.Id, session.QueueEntry.PurchaseIdentityHash, Now, ct);

        var limit = await persistence.PurchaseLimitAsync(eventId, product.Id, tenant, ct);

        if (quantity + used > limit || quantity > int.MaxValue)
            throw Failure("DropPurchase.PurchaseLimit", "A soma das variações e das reservas deste CPF atingiu o limite do produto no evento.");

        var total = variants.Sum(Item => Item.Variant!.Price * Item.Line.Quantity);

        if (total <= 0 || total > 9999999999999999.99m)
            throw Failure("DropPurchase.InvalidTotal", "O total da reserva excede a capacidade de cobrança.");

        var reservation = DropReservation.Create(eventId, participation.Id, customer, session.QueueEntryId, 1, (int)quantity,
            decimal.Round(total / quantity, 2), total, OpaqueToken.Hash(OpaqueToken.Create()), Now, session.ExpiresAt, null, null);
        reservation.BindSession(session.Id, session.QueueEntry.PurchaseIdentityHash);
        reservation.SetIdempotencyKey(request.IdempotencyKey);

        foreach (var item in variants)
        {
            item.Variant!.Reserve(item.Line.Quantity);
            reservation.Lines.Add(DropReservationLine.Capture(reservation, item.Variant, item.Line.Quantity));
        }

        persistence.Add(reservation);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<DropReservationResponse>.Success(Capture(reservation));
    }

    private static SecurityFailure Failure(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }

    private static DropReservationResponse Capture(DropReservation reservation)
    {
        return new DropReservationResponse(
            reservation.Id, reservation.DropEventId, reservation.DropProductId, reservation.Quantity,
            reservation.TotalAmount, reservation.ExpiresAt,
            reservation.Lines.Select(Line => new DropReservationLineResponse(
                Line.VariantId, Line.SKU, Line.ItemName, Line.Quantity, Line.UnitPrice, Line.OptionsJson)));
    }
}
