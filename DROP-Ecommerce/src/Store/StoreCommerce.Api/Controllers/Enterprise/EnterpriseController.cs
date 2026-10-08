using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/enterprises")]
public class EnterpriseController : BaseController<Enterprise, CreateEnterpriseCommand, CreateListEnterpriseCommand, UpdateEnterpriseCommand, UpdateListEnterpriseCommand>
{
    public EnterpriseController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListEnterpriseCommand WrapCreateInRange(CreateEnterpriseCommand command)
    {
        return new CreateListEnterpriseCommand(new List<CreateEnterpriseCommand> { command });
    }

    protected override UpdateListEnterpriseCommand WrapUpdateInRange(UpdateEnterpriseCommand command)
    {
        return new UpdateListEnterpriseCommand(new List<UpdateEnterpriseCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListEnterpriseCommand(ids);
    }

    protected override IRequest<Result<List<Enterprise>>> GetAllQuery()
    {
        return new GetAllEnterpriseQuery();
    }

    protected override IRequest<Result<Enterprise>> GetByIdQuery(long id)
    {
        return new GetByIdEnterpriseQuery(id);
    }

    protected override IRequest<Result<List<Enterprise>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdEnterpriseQuery(ids);
    }
}
