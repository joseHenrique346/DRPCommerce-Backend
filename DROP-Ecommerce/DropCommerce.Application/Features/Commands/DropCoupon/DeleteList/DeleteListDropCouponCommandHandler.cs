using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropCouponCommandHandler(IRepository<DropCoupon> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropCouponCommand, DropCoupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropCouponCommand request) => request.ids;
}
