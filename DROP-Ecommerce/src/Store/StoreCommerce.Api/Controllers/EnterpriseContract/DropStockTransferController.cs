using MediatR;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Api.Extensions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/products/{id:long}/drop-stock-transfer")]
[Authorize(Policy = SecurityNames.Backoffice)]
public sealed class DropStockTransferController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Transfer(long id, StartDropStockTransfer request, CancellationToken ct)
    {
        var result = await mediator.Send(new StartDropStockTransferCommand(id, request), ct);

        return result.IsSuccess ? Accepted(result.Content) : this.ToIdentityActionResult(result);
    }
}
