using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListEmployeePositionCommandHandler(IEmployeePositionRepository repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListEmployeePositionCommand, EmployeePosition>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListEmployeePositionCommand request) => request.ids;
}
