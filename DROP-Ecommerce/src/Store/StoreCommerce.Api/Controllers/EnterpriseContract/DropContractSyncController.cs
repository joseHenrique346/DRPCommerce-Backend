using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Security;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprises/current/contract/drop-sync")]
[Authorize(Policy = SecurityNames.PrimaryOwner)]
public sealed class DropContractSyncController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await mediator.Send(new GetDropContractSyncQuery(), ct);

        return this.ToIdentityActionResult(result);
    }
}
