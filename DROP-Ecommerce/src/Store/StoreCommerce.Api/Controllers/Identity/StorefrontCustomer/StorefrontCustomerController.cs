using SharedCommerce.Application.Security;
using MediatR;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Features.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Api.Security;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/{storeSlug}")]
[EnableRateLimiting("api")]
public sealed class StorefrontCustomerController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Consulta o perfil do cliente autenticado nesta loja.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetCustomerQuery(), ct));
    }

    /// <summary>
    /// Atualiza o perfil do cliente autenticado nesta loja.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpPut("me")]
    public async Task<IActionResult> Update(CustomerProfileRequest request, CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new UpdateCustomerProfileCommand(request), ct));
    }

    /// <summary>
    /// Lista os endereços de entrega do cliente (o padrão primeiro).
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpGet("me/addresses")]
    public async Task<IActionResult> Addresses(CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new GetMyAddressesQuery(), ct));
    }

    /// <summary>
    /// Cadastra um endereço de entrega. O primeiro endereço vira o padrão.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpPost("me/addresses")]
    public async Task<IActionResult> AddAddress([FromBody] CustomerAddressRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new AddMyAddressCommand(request), ct));
    }

    /// <summary>
    /// Atualiza um endereço de entrega. Pedidos já feitos mantêm o endereço da compra.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpPut("me/addresses/{id:long}")]
    public async Task<IActionResult> UpdateAddress(long id, [FromBody] CustomerAddressRequest request, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new UpdateMyAddressCommand(id, request), ct));
    }

    /// <summary>
    /// Define o endereço padrão do checkout.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpPut("me/addresses/{id:long}/default")]
    public async Task<IActionResult> SetDefaultAddress(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new SetMyDefaultAddressCommand(id), ct));
    }

    /// <summary>
    /// Exclui um endereço. Se era o padrão, o mais recente que restar vira o padrão.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpDelete("me/addresses/{id:long}")]
    public async Task<IActionResult> DeleteAddress(long id, CancellationToken ct)
    {
        return this.ToActionResult(await mediator.Send(new DeleteMyAddressCommand(id), ct));
    }

    /// <summary>
    /// Informa pagamentos, operações incertas ou pedidos em andamento que impedem a exclusão nesta loja.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpGet("me/deletion")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetDeletionEligibility(CancellationToken ct)
    {
        return this.ToIdentityActionResult(await mediator.Send(new GetAccountDeletionEligibilityQuery(true), ct));
    }

    /// <summary>
    /// Apaga o perfil nesta loja mediante senha atual. Mantém registros e snapshots comerciais, sem apagar vendas.
    /// </summary>
    [Authorize(Policy = SecurityNames.Storefront)]
    [HttpDelete("me")]
    [CookieCsrf]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Delete([FromBody] DeleteAccountRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateCustomerAccountCommand(request), ct);

        if (result.IsFailure)
            return this.ToIdentityActionResult(result);

        Response.ClearRefreshCookie(true);

        return NoContent();
    }
}
