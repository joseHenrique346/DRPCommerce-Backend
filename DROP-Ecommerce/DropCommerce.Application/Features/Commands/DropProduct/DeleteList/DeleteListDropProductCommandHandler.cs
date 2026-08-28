using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropProductCommandHandler(IRepository<DropProduct> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropProductCommand, DropProduct>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropProductCommand request) => request.ids;
}
