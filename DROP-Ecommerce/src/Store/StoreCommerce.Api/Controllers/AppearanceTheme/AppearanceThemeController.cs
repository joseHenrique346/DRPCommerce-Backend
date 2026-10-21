using Commerce.Appearance.Application.Features;
using Commerce.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/appearance-themes")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
[EnableRateLimiting("api")]
public sealed class AppearanceThemeController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista os temas disponíveis para a vitrine.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Commerce.Appearance.Contracts.OutputThemeDescription>), 200)]
    public async Task<IActionResult> GetThemes(CancellationToken ct)
    {
        return Ok(await mediator.Send(new GetAppearanceThemesQuery(), ct));
    }
}
