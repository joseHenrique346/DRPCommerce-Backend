using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/transactions")]
public class TransactionController : BaseController<Transaction, CreateTransactionCommand, CreateListTransactionCommand, UpdateTransactionCommand, UpdateListTransactionCommand>
{
    public TransactionController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListTransactionCommand WrapCreateInRange(CreateTransactionCommand command)
    {
        return new CreateListTransactionCommand(new List<CreateTransactionCommand> { command });
    }

    protected override UpdateListTransactionCommand WrapUpdateInRange(UpdateTransactionCommand command)
    {
        return new UpdateListTransactionCommand(new List<UpdateTransactionCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListTransactionCommand(ids);
    }

    protected override IRequest<Result<List<Transaction>>> GetAllQuery()
    {
        return new GetAllTransactionQuery();
    }

    protected override IRequest<Result<Transaction>> GetByIdQuery(long id)
    {
        return new GetByIdTransactionQuery(id);
    }

    protected override IRequest<Result<List<Transaction>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdTransactionQuery(ids);
    }
}
