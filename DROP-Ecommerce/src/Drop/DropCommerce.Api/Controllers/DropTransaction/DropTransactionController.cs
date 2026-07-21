using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-transactions")]
public class DropTransactionController : BaseController<DropTransaction, CreateDropTransactionCommand, CreateListDropTransactionCommand, UpdateDropTransactionCommand, UpdateListDropTransactionCommand>
{
    public DropTransactionController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropTransactionCommand WrapCreateInRange(CreateDropTransactionCommand command)
    {
        return new CreateListDropTransactionCommand(new List<CreateDropTransactionCommand> { command });
    }

    protected override UpdateListDropTransactionCommand WrapUpdateInRange(UpdateDropTransactionCommand command)
    {
        return new UpdateListDropTransactionCommand(new List<UpdateDropTransactionCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropTransactionCommand(ids);
    }

    protected override IRequest<Result<List<DropTransaction>>> GetAllQuery()
    {
        return new GetAllDropTransactionQuery();
    }

    protected override IRequest<Result<DropTransaction>> GetByIdQuery(long id)
    {
        return new GetByIdDropTransactionQuery(id);
    }

    protected override IRequest<Result<List<DropTransaction>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropTransactionQuery(ids);
    }
}
