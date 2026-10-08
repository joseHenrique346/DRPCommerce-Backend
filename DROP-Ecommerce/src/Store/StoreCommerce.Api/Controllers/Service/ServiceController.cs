using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/services")]
public class ServiceController : BaseController<Service, CreateServiceCommand, CreateListServiceCommand, UpdateServiceCommand, UpdateListServiceCommand>
{
    public ServiceController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListServiceCommand WrapCreateInRange(CreateServiceCommand command)
    {
        return new CreateListServiceCommand(new List<CreateServiceCommand> { command });
    }

    protected override UpdateListServiceCommand WrapUpdateInRange(UpdateServiceCommand command)
    {
        return new UpdateListServiceCommand(new List<UpdateServiceCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListServiceCommand(ids);
    }

    protected override IRequest<Result<List<Service>>> GetAllQuery()
    {
        return new GetAllServiceQuery();
    }

    protected override IRequest<Result<Service>> GetByIdQuery(long id)
    {
        return new GetByIdServiceQuery(id);
    }

    protected override IRequest<Result<List<Service>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdServiceQuery(ids);
    }
}
