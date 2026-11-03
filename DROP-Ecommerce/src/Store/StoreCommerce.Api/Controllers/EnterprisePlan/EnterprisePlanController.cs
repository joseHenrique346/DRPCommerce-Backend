using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Api.Controllers;

[ApiController]
[Route("api/enterprise-plans")]
public class EnterprisePlanController : ControllerBase
{
    /// <summary>
    /// Lista os planos disponíveis para contratação com suas capacidades.
    /// </summary>
    [HttpGet("get-all")]
    public IActionResult GetAll() => Ok(Result<List<EnterprisePlan>>.Success(
        BaseStaticEntity.GetAll<EnterprisePlan>().ToList()));
}
