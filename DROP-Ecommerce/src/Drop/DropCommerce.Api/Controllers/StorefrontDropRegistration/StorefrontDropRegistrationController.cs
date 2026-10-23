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
public sealed class StorefrontDropRegistrationController(DropCommerceDbContext db, IDropOperationScope scope, TimeProvider time) : ControllerBase
{
    private long CustomerId => SecurityNames.Id(User, SecurityNames.Customer);
    private long EnterpriseId => SecurityNames.Id(User, SecurityNames.Enterprise);
    private DateTime Now => time.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Inscreve o cliente em um evento de drop.
    /// </summary>
    [HttpPost("events/{id:long}/register")]
    public async Task<IActionResult> Register(long id, CancellationToken ct)
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

        if (now < drop.RegistrationStartsAt || now > drop.RegistrationEndsAt)
            throw new SecurityFailure(409, "Drop.RegistrationClosed", "Inscrições fora do período permitido.");

        var registration = await db.DropRegistrations
            .SingleOrDefaultAsync(x => x.DropEventId == id && x.CustomerId == customerId, ct);

        if (registration is not null)
            return Ok(new
            {
                registration.Id,
                registration.DropRegistrationStatusId,
                registration.IsEligible
            });

        registration = DropRegistration.Create(
            id,
            customerId,
            DropRegistrationStatus.Pending.Id,
            false,
            "Aguardando avaliação de elegibilidade",
            now,
            null);

        db.DropRegistrations.Add(registration);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new
        {
            registration.Id,
            registration.DropRegistrationStatusId,
            registration.IsEligible
        });
    }

    /// <summary>
    /// Lista as inscrições de drop do cliente.
    /// </summary>
    [HttpGet("registrations")]
    public async Task<IActionResult> Registrations(CancellationToken ct)
    {
        return Ok(await db.DropRegistrations
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.DropEventId,
                x.DropRegistrationStatusId,
                x.IsEligible,
                x.RegisteredAt
            })
            .ToListAsync(ct));
    }
}

