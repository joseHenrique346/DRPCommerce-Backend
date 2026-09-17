using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListDocumentCommandHandler(IRepository<Document> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDocumentCommand, UpdateListDocumentCommand, Document>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDocumentCommand> GetCommandList(UpdateListDocumentCommand request) => request.commands;

    protected override long GetById(UpdateDocumentCommand command) => command.id;

    protected override void ApplyChanges(Document entity, UpdateDocumentCommand command)
    {
        entity.UpdateDetails(command.referenceId, command.referenceType, command.typeId, command.number, command.fileUrl, command.statusId, command.issuedAt, command.expiresAt);
    }
}
