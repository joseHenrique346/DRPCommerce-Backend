using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListFraudSignalCommandHandler(IRepository<FraudSignal> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListFraudSignalCommand, FraudSignal>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListFraudSignalCommand request) => request.ids;
}
