using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListWaitlistEntryCommandHandler(IRepository<WaitlistEntry> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListWaitlistEntryCommand, WaitlistEntry>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListWaitlistEntryCommand request) => request.ids;
}
