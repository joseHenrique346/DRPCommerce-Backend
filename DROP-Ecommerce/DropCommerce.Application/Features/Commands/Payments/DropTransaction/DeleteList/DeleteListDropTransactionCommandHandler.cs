using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropTransactionCommandHandler(IRepository<DropTransaction> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropTransactionCommand, DropTransaction>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropTransactionCommand request) => request.ids;
}
