using Commerce.Appearance.Api;
using Commerce.Appearance.Contracts;
using StoreCommerce.Application.Appearance;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/appearances")]
public sealed class AppearanceController(IMediator mediator) : AppearanceAdminControllerBase(mediator)
{
    protected override InputAppearanceTarget Target => StoreAppearanceTargets.Default;
}
