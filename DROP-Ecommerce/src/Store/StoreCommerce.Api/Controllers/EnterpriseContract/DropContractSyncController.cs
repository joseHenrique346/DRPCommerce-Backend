using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Api.Controllers;

public sealed record AcknowledgeDropContractRequest(long Revision);

[ApiController]
[Route("api/internal")]
[AllowAnonymous]
public sealed class InternalDropContractController(StoreCommerceDbContext db,
    IIdentityOperationScope identity, DropContractAcknowledgmentScope acknowledgments) : ControllerBase
{
    [HttpGet("enterprises/{id:long}/drop-contract")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant || tenant != id)
            return NotFound();

        var enterprise = await db.Enterprises.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Enterprise => Enterprise.Id == id && Enterprise.IsActive, ct);

        return enterprise is null ? NotFound() : Ok(Result<DropCommercialRights>.Success(Rights(enterprise)));
    }

    [HttpGet("drop-contracts/pending")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        var changes = await db.Set<DropContractPublication>().IgnoreQueryFilters().AsNoTracking()
            .Where(Publication => Publication.AcknowledgedRevision < Publication.Revision && Publication.Enterprise.IsActive)
            .OrderBy(Publication => Publication.ChangedAt).ThenBy(Publication => Publication.Id)
            .Select(Publication => Publication.Enterprise).Take(50).ToListAsync(ct);

        return Ok(Result<IReadOnlyList<DropCommercialRights>>.Success(changes.Select(Rights).ToList()));
    }

    [HttpPost("enterprises/{id:long}/drop-contract/ack")]
    public async Task<IActionResult> Acknowledge(long id, AcknowledgeDropContractRequest request, CancellationToken ct)
    {
        if (HttpContext.Items[SecurityNames.TrustedServiceTenant] is not long tenant || tenant != id)
            return NotFound();

        using var operation = identity.Begin();
        using var acknowledgment = acknowledgments.Begin();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {id} FOR UPDATE", ct);
        var publication = await db.Set<DropContractPublication>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(Publication => Publication.EnterpriseId == id, ct);

        if (publication is null || request.Revision < 1 || request.Revision > publication.Revision)
            return Conflict();

        publication.Acknowledge(request.Revision, DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return NoContent();
    }

    private static DropCommercialRights Rights(Enterprise enterprise)
    {
        var rights = EnterpriseEntitlements.Resolve(enterprise.PlanId, enterprise.AddOnId);

        return new DropCommercialRights(enterprise.Id, enterprise.ContractRevision, enterprise.CatalogVersion,
            rights.AllowsDrop, rights.DropProductLimit);
    }
}

[ApiController]
[Route("api/enterprises/current/contract/drop-sync")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class DropContractSyncController(StoreCommerceDbContext db, ICurrentIdentity actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        actor.RequirePrimaryOwner();
        var tenant = actor.EnterpriseId;
        var state = await db.Set<DropContractPublication>().IgnoreQueryFilters().AsNoTracking()
            .Where(Publication => Publication.EnterpriseId == tenant)
            .Select(Publication => new { sourceRevision = Publication.Revision, Publication.AcknowledgedRevision,
                pending = Publication.AcknowledgedRevision < Publication.Revision, Publication.ChangedAt, Publication.AcknowledgedAt })
            .SingleOrDefaultAsync(ct);

        return Ok(state);
    }
}
