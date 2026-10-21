using System.Globalization;
using Commerce.Appearance.Api;
using Commerce.Appearance.Contracts;
using DropCommerce.Application.Appearance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-events/{eventId:long:min(1)}/appearance")]
public sealed class DropEventAppearanceController(IMediator mediator) : AppearanceAdminControllerBase(mediator)
{
    protected override InputAppearanceTarget Target => DropAppearanceTargets.Event(
        long.Parse(RouteData.Values["eventId"]!.ToString()!, CultureInfo.InvariantCulture));
}
