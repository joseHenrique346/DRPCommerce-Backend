using MediatR;
using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ValidateCouponQueryHandler(ICouponRepository couponRepository, IRepository<Customer> customerRepository, CouponEligibilityService eligibility, TimeProvider time) : IRequestHandler<ValidateCouponQuery, Result<CouponValidationResponse>>
{
    public async Task<Result<CouponValidationResponse>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var code = Coupon.NormalizeCode(request.Code);
        var coupon = await couponRepository.GetByCodeAsync(code, cancellationToken);

        if (coupon is null)
            return Result<CouponValidationResponse>.Success(new(code, false, 0, request.SubTotal, "Coupon.NotFound", "Cupom não encontrado.", []));

        CouponCustomerHistory? history = null;

        if (request.CustomerId is long customerId)
        {
            if (!await customerRepository.ExistsAsync(customer => customer.Id == customerId && customer.IsActive, cancellationToken))
                return Result<CouponValidationResponse>.NotFound("Customer.NotFound", $"Customer com id {customerId} não encontrado.");

            history = await eligibility.GetHistoryAsync(coupon.Id, customerId, null, cancellationToken);
        }

        var evaluation = coupon.Evaluate(time.GetUtcNow().UtcDateTime, request.SubTotal, request.ItemQuantity, history);

        return Result<CouponValidationResponse>.Success(new(
            coupon.Code,
            evaluation.IsValid,
            evaluation.DiscountAmount,
            request.SubTotal - evaluation.DiscountAmount,
            evaluation.ErrorCode,
            evaluation.Message,
            coupon.ListCondition.Select(CouponConditionResponse.FromEntity).ToList()));
    }
}
