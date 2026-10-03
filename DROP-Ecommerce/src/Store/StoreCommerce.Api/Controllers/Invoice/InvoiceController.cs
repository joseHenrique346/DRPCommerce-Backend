using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/invoices")]
public class InvoiceController : BaseController<Invoice, CreateInvoiceCommand, CreateListInvoiceCommand, UpdateInvoiceCommand, UpdateListInvoiceCommand>
{
    public InvoiceController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListInvoiceCommand WrapCreateInRange(CreateInvoiceCommand command)
    {
        return new CreateListInvoiceCommand(new List<CreateInvoiceCommand> { command });
    }

    protected override UpdateListInvoiceCommand WrapUpdateInRange(UpdateInvoiceCommand command)
    {
        return new UpdateListInvoiceCommand(new List<UpdateInvoiceCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListInvoiceCommand(ids);
    }

    protected override IRequest<Result<List<Invoice>>> GetAllQuery()
    {
        return new GetAllInvoiceQuery();
    }

    protected override IRequest<Result<Invoice>> GetByIdQuery(long id)
    {
        return new GetByIdInvoiceQuery(id);
    }

    protected override IRequest<Result<List<Invoice>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdInvoiceQuery(ids);
    }
}
