using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/suppliers")]
public class SupplierController : BaseController<Supplier, CreateSupplierCommand, CreateListSupplierCommand, UpdateSupplierCommand, UpdateListSupplierCommand>
{
    public SupplierController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListSupplierCommand WrapCreateInRange(CreateSupplierCommand command)
    {
        return new CreateListSupplierCommand(new List<CreateSupplierCommand> { command });
    }

    protected override UpdateListSupplierCommand WrapUpdateInRange(UpdateSupplierCommand command)
    {
        return new UpdateListSupplierCommand(new List<UpdateSupplierCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListSupplierCommand(ids);
    }

    protected override IRequest<Result<List<Supplier>>> GetAllQuery()
    {
        return new GetAllSupplierQuery();
    }

    protected override IRequest<Result<Supplier>> GetByIdQuery(long id)
    {
        return new GetByIdSupplierQuery(id);
    }

    protected override IRequest<Result<List<Supplier>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdSupplierQuery(ids);
    }
}
