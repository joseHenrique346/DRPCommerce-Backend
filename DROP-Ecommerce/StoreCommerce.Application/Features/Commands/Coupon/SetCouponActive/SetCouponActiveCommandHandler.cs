using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class SetCouponActiveCommandHandler(IRepository<Coupon> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<SetCouponActiveCommand, Result<Coupon>>
{
    public async Task<Result<Coupon>> Handle(SetCouponActiveCommand request, CancellationToken cancellationToken)
    {
        var coupon = await repository.GetByIdAsync(request.id, cancellationToken);

        if (coupon is null)
            return Result<Coupon>.NotFound("Coupon.NotFound", $"Coupon com id {request.id} não encontrado.");

        coupon.SetActive(request.active);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Coupon>.Success(coupon);
    }
}
