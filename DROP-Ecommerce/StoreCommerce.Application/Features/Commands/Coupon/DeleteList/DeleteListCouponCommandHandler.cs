using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListCouponCommandHandler(IRepository<Coupon> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListCouponCommand, Coupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListCouponCommand request) => request.ids;
}
