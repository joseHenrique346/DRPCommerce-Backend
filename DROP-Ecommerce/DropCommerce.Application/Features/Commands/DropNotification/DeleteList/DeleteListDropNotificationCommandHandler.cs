using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropNotificationCommandHandler(IRepository<DropNotification> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropNotificationCommand, DropNotification>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropNotificationCommand request) => request.ids;
}
