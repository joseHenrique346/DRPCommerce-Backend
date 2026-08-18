using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/fraud-signals")]
public class FraudSignalController : BaseController<FraudSignal, CreateFraudSignalCommand, CreateListFraudSignalCommand, UpdateFraudSignalCommand, UpdateListFraudSignalCommand>
{
    public FraudSignalController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListFraudSignalCommand WrapCreateInRange(CreateFraudSignalCommand command)
    {
        return new CreateListFraudSignalCommand(new List<CreateFraudSignalCommand> { command });
    }

    protected override UpdateListFraudSignalCommand WrapUpdateInRange(UpdateFraudSignalCommand command)
    {
        return new UpdateListFraudSignalCommand(new List<UpdateFraudSignalCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListFraudSignalCommand(ids);
    }

    protected override IRequest<Result<List<FraudSignal>>> GetAllQuery()
    {
        return new GetAllFraudSignalQuery();
    }

    protected override IRequest<Result<FraudSignal>> GetByIdQuery(long id)
    {
        return new GetByIdFraudSignalQuery(id);
    }

    protected override IRequest<Result<List<FraudSignal>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdFraudSignalQuery(ids);
    }
}
