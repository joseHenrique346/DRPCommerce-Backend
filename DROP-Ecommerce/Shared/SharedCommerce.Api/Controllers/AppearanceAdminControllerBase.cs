using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Features.Commands;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Api.Controllers;

[ApiController]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
[EnableRateLimiting("api")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public abstract class AppearanceAdminControllerBase(IMediator mediator) : ControllerBase
{
    protected abstract InputAppearanceTarget Target { get; }

    /// <summary>
    /// Consulta o estado atual da aparência.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AppearanceStatusResponse), 200)]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetAppearanceStatusQuery(Target), ct));
    }

    /// <summary>
    /// Consulta a sessão de edição da aparência.
    /// </summary>
    [HttpGet("edit-session")]
    [ProducesResponseType(typeof(AppearanceEditResponse), 200)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetEditSession(CancellationToken ct)
    {
        var session = await mediator.Send(new GetAppearanceEditSessionQuery(Target), ct);

        return session is null ? NoContent() : EditResult(session);
    }

    /// <summary>
    /// Inicia uma sessão de edição da aparência.
    /// </summary>
    [HttpPost("edit-sessions")]
    [ProducesResponseType(typeof(AppearanceEditResponse), 200)]
    public async Task<IActionResult> BeginEdit(BeginAppearanceEditRequest request, CancellationToken ct)
    {
        return EditResult(await mediator.Send(new BeginAppearanceEditCommand(Target, request), ct));
    }

    /// <summary>
    /// Salva as configurações da aparência em edição.
    /// </summary>
    [HttpPut("edit-sessions/{sessionId:long:min(1)}/settings")]
    [ProducesResponseType(typeof(AppearanceEditResponse), 200)]
    public async Task<IActionResult> Save(long sessionId, SaveAppearanceRequest request,
        [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken ct)
    {
        return EditResult(await mediator.Send(new SaveAppearanceCommand(Target, sessionId, ExpectedVersion(), request), ct));
    }

    /// <summary>
    /// Gera uma prévia da aparência em edição.
    /// </summary>
    [HttpPost("edit-sessions/{sessionId:long:min(1)}/preview")]
    [ProducesResponseType(typeof(AppearancePreviewResponse), 200)]
    public async Task<IActionResult> Preview(long sessionId, [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken ct)
    {
        var response = await mediator.Send(new PreviewAppearanceCommand(Target, sessionId, ExpectedVersion()), ct);
        Response.Headers.ETag = Quote(response.Session.Version);

        return Ok(response);
    }

    /// <summary>
    /// Publica a aparência em edição.
    /// </summary>
    [HttpPost("edit-sessions/{sessionId:long:min(1)}/publish")]
    [ProducesResponseType(typeof(AppearanceManifest), 200)]
    public async Task<IActionResult> Publish(long sessionId, [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken ct)
    {
        return Ok(await mediator.Send(new PublishAppearanceCommand(Target, sessionId, ExpectedVersion()), ct));
    }

    /// <summary>
    /// Cancela a sessão de edição da aparência.
    /// </summary>
    [HttpPost("edit-sessions/{sessionId:long:min(1)}/cancel")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Cancel(long sessionId, [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken ct)
    {
        await mediator.Send(new CancelAppearanceCommand(Target, sessionId, ExpectedVersion()), ct);

        return NoContent();
    }

    /// <summary>
    /// Libera a sessão de edição da aparência.
    /// </summary>
    [HttpPost("edit-sessions/{sessionId:long:min(1)}/release")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Release(long sessionId, [FromHeader(Name = "If-Match")] string? ifMatch, CancellationToken ct)
    {
        await mediator.Send(new CancelAppearanceCommand(Target, sessionId, ExpectedVersion(), true), ct);

        return NoContent();
    }

    /// <summary>
    /// Lista o histórico de revisões da aparência.
    /// </summary>
    [HttpGet("revisions")]
    [ProducesResponseType(typeof(IReadOnlyList<AppearanceRevisionResponse>), 200)]
    public async Task<IActionResult> GetRevisions([FromQuery] long? beforeNumber, CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetAppearanceRevisionsQuery(Target, beforeNumber), ct));
    }

    /// <summary>
    /// Restaura uma revisão anterior da aparência.
    /// </summary>
    [HttpPost("revisions/{revisionId:long:min(1)}/rollback")]
    [ProducesResponseType(typeof(AppearanceManifest), 200)]
    public async Task<IActionResult> Rollback(long revisionId, RollbackAppearanceRequest request, CancellationToken ct)
    {
        return Ok(await mediator.Send(new RollbackAppearanceCommand(Target, revisionId, request), ct));
    }

    /// <summary>
    /// Tenta novamente a migração da aparência.
    /// </summary>
    [HttpPost("migration/retry")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RetryMigration(CancellationToken ct)
    {
        await mediator.Send(new RetryAppearanceMigrationCommand(Target), ct);

        return NoContent();
    }

    /// <summary>
    /// Verifica a compatibilidade da aparência para migração.
    /// </summary>
    [HttpGet("migration/preflight")]
    [ProducesResponseType(typeof(AppearanceMigrationPreflightResponse), 200)]
    public async Task<IActionResult> GetMigrationPreflight(CancellationToken ct)
    {
        return Ok(await mediator.Send(new PreflightAppearanceMigrationQuery(Target), ct));
    }

    private IActionResult EditResult(AppearanceEditResponse response)
    {
        Response.Headers.ETag = Quote(response.Version);

        return Ok(response);
    }

    private long ExpectedVersion()
    {
        var value = Request.Headers.IfMatch.ToString();

        if (string.IsNullOrEmpty(value))
            throw new AppearanceFailure(428, "Appearance.VersionRequired", "Envie o ETag da sessão no header If-Match.");

        if (value.Length < 3 || value[0] != '"' || value[^1] != '"' ||
            !long.TryParse(value.AsSpan(1, value.Length - 2), NumberStyles.None, CultureInfo.InvariantCulture, out var version) || version < 1)
            throw new AppearanceFailure(400, "Appearance.InvalidVersion", "O header If-Match deve conter uma versão forte de sessão.");

        return version;
    }

    private static string Quote(long version)
    {
        return "\"" + version.ToString(CultureInfo.InvariantCulture) + "\"";
    }
}