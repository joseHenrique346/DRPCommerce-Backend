using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropTransactionCommandHandler(IRepository<DropTransaction> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropTransactionCommand, DropTransaction>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropTransactionCommand request) => request.ids;
}
