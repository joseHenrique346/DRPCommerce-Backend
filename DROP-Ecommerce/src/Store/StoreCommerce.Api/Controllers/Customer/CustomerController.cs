using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Api.Extensions;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/customers")]
public class CustomerController : MappedCrudController<Customer, CustomerResponse, CreateCustomerCommand, CreateListCustomerCommand, UpdateCustomerCommand, UpdateListCustomerCommand>
{
    public CustomerController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Busca clientes por nome, e-mail ou telefone, situação, UF, período de cadastro e histórico de compras, com paginação.
    /// Traz quantidade de compras concluídas, total gasto e data da última compra. Os pedidos de um cliente ficam em
    /// GET api/orders/search?customerId=.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> GetSearchAsync([FromQuery] SearchCustomersQuery query, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(query, ct));
    }

    /// <summary>
    /// Lista os endereços de entrega salvos do cliente.
    /// </summary>
    [HttpGet("{id:long}/addresses")]
    public async Task<IActionResult> GetAddressesAsync(long id, CancellationToken ct)
    {
        return this.ToActionResult(await _mediator.Send(new GetCustomerAddressesQuery(id), ct));
    }

    protected override CustomerResponse MapEntity(Customer entity) => new(
        entity.Id, entity.EnterpriseId, entity.FullName, entity.Email.Value,
        entity.Phone.Value, entity.NormalizedEmail, entity.EmailConfirmed,
        entity.AddressLine, entity.City, entity.StateId, entity.ZipCode,
        entity.Country, entity.Gender, entity.DateOfBirth, entity.IsVerified,
        entity.IsActive, entity.IsDeleted, entity.DeletedAt, entity.CreatedAt, entity.UpdatedAt);

    protected override CreateListCustomerCommand WrapCreateInRange(CreateCustomerCommand command)
    {
        return new CreateListCustomerCommand(new List<CreateCustomerCommand> { command });
    }

    protected override UpdateListCustomerCommand WrapUpdateInRange(UpdateCustomerCommand command)
    {
        return new UpdateListCustomerCommand(new List<UpdateCustomerCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListCustomerCommand(ids);
    }

    protected override IRequest<Result<List<Customer>>> GetAllQuery()
    {
        return new GetAllCustomerQuery();
    }

    protected override IRequest<Result<Customer>> GetByIdQuery(long id)
    {
        return new GetByIdCustomerQuery(id);
    }

    protected override IRequest<Result<List<Customer>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdCustomerQuery(ids);
    }
}
