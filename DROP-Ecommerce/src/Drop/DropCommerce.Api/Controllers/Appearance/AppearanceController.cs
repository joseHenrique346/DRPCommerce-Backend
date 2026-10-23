using SharedCommerce.Api.Controllers;
using SharedCommerce.Application.Contracts.Appearance;
using DropCommerce.Application.Appearance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/appearances")]
public sealed class AppearanceController(IMediator mediator) : AppearanceAdminControllerBase(mediator)
{
    protected override InputAppearanceTarget Target => DropAppearanceTargets.Default;
}
