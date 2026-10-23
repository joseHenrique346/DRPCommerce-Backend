using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using DropCommerce.Application.Security;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.StaticEntity;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontQueueEntryController(DropCommerceDbContext db, IDropOperationScope scope, TimeProvider time) : ControllerBase
{
    private long CustomerId => SecurityNames.Id(User, SecurityNames.Customer);
    private long EnterpriseId => SecurityNames.Id(User, SecurityNames.Enterprise);
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Adiciona o cliente à fila do evento de drop.
    /// </summary>
    [HttpPost("events/{id:long}/queue")]
    public async Task<IActionResult> JoinQueue(long id, CancellationToken ct)
    {
        var customerId = CustomerId;
        var tenant = EnterpriseId;
        using var operation = scope.Begin();

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT id FROM evento WHERE id = {id} AND enterprise_id = {tenant} FOR UPDATE",
            ct);

        var drop = await db.DropEvents
            .SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new SecurityFailure(404, "Drop.NotFound", "Evento indisponível.");
        var now = Now;

        if (now < drop.QueueOpensAt || now >= drop.DropEndsAt)
            throw new SecurityFailure(409, "Drop.QueueClosed", "Fila fora do período permitido.");

        if (drop.RequiresRegistration &&
            !await db.DropRegistrations.AnyAsync(
                x => x.DropEventId == id && x.CustomerId == customerId && x.IsEligible,
                ct))
            throw new SecurityFailure(403, "Drop.RegistrationRequired", "Inscrição elegível necessária.");

        var existing = await db.QueueEntries
            .SingleOrDefaultAsync(x => x.DropEventId == id && x.CustomerId == customerId, ct);

        if (existing is not null)
            return Ok(new
            {
                existing.Id,
                existing.Position,
                existing.QueueEntryStatusId
            });

        // Count across the whole event, not the customer's query-filtered view.
        var position = (await db.QueueEntries
            .IgnoreQueryFilters()
            .Where(x => x.DropEventId == id)
            .MaxAsync(x => (int?)x.Position, ct) ?? 0) + 1;
        var userAgent = Request.Headers.UserAgent.ToString();

        if (string.IsNullOrWhiteSpace(userAgent))
            userAgent = "unknown";

        var entry = QueueEntry.Create(
            id,
            customerId,
            OpaqueToken.Hash(OpaqueToken.Create()),
            position,
            QueueEntryStatus.Waiting.Id,
            "not-provided",
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            userAgent[..Math.Min(userAgent.Length, 500)],
            now,
            null,
            null,
            null);

        db.QueueEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        var session = QueueSession.Create(
            entry.Id,
            customerId,
            OpaqueToken.Hash(OpaqueToken.Create()),
            QueueSessionStatus.Active.Id,
            now,
            drop.DropEndsAt,
            now);

        db.QueueSessions.Add(session);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new
        {
            entry.Id,
            entry.Position,
            entry.QueueEntryStatusId,
            SessionId = session.Id,
            session.ExpiresAt
        });
    }

    /// <summary>
    /// Lista as entradas do cliente nas filas de drop.
    /// </summary>
    [HttpGet("queue")]
    public async Task<IActionResult> Queue(CancellationToken ct)
    {
        return Ok(await db.QueueEntries
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.DropEventId,
                x.Position,
                x.QueueEntryStatusId,
                x.EnteredAt
            })
            .ToListAsync(ct));
    }
}

