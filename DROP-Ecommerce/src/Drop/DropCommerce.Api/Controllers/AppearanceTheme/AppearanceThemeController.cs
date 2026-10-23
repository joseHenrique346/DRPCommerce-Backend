using SharedCommerce.Application.Security;
using SharedCommerce.Application.Features.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/appearance-themes")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
[EnableRateLimiting("api")]
public sealed class AppearanceThemeController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os temas disponíveis para os drops.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SharedCommerce.Application.Contracts.Appearance.OutputThemeDescription>), 200)]
    public async Task<IActionResult> GetThemes(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetAppearanceThemesQuery(), ct));
    }
}
