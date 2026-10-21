using Commerce.Appearance.Api;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/internal/appearance-themes")]
public sealed class InternalAppearanceThemeController(IMediator mediator) : AppearanceThemeCatalogControllerBase(mediator)
{
}
