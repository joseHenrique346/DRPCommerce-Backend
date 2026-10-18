using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprise-membership-kinds")]
public class EnterpriseMembershipKindController : ControllerBase
{
    /// <summary>
    /// Lista os tipos de vínculo com a loja.
    /// </summary>
    [HttpGet("get-all")]
    public IActionResult GetAll() => Ok(Result<List<EnterpriseMembershipKind>>.Success(
        BaseStaticEntity.GetAll<EnterpriseMembershipKind>().ToList()));
}
