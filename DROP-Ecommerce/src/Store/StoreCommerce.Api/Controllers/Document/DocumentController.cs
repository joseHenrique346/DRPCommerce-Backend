using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/documents")]
public class DocumentController : BaseController<Document, CreateDocumentCommand, CreateListDocumentCommand, UpdateDocumentCommand, UpdateListDocumentCommand>
{
    public DocumentController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDocumentCommand WrapCreateInRange(CreateDocumentCommand command)
    {
        return new CreateListDocumentCommand(new List<CreateDocumentCommand> { command });
    }

    protected override UpdateListDocumentCommand WrapUpdateInRange(UpdateDocumentCommand command)
    {
        return new UpdateListDocumentCommand(new List<UpdateDocumentCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDocumentCommand(ids);
    }

    protected override IRequest<Result<List<Document>>> GetAllQuery()
    {
        return new GetAllDocumentQuery();
    }

    protected override IRequest<Result<Document>> GetByIdQuery(long id)
    {
        return new GetByIdDocumentQuery(id);
    }

    protected override IRequest<Result<List<Document>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDocumentQuery(ids);
    }
}
