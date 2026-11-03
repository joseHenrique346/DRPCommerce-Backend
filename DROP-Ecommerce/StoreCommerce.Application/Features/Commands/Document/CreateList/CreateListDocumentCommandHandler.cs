using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListDocumentCommandHandler(IRepository<Document> repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateDocumentCommand, CreateListDocumentCommand, Document>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDocumentCommand> GetCommandList(CreateListDocumentCommand request) => request.commands;

    protected override Document CreateEntity(CreateDocumentCommand command) =>
        Document.Create(tenantProvider.GetEnterpriseId(), command.referenceId, command.referenceType, command.typeId, command.number, command.fileUrl, command.statusId, command.issuedAt, command.expiresAt);
}
