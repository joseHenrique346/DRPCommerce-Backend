using StoreCommerce.Api.Controllers.Base;
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
