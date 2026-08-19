using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropOrderCommandValidator : AbstractValidator<UpdateDropOrderCommand>
{
    public UpdateDropOrderCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
        RuleFor(x => x.dropEventId).GreaterThan(0);
        RuleFor(x => x.customerId).GreaterThan(0);
        RuleFor(x => x.reservationId).GreaterThan(0);
        RuleFor(x => x.couponId).GreaterThan(0).When(x => x.couponId.HasValue);
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.paymentStatusId).GreaterThan(0);
        RuleFor(x => x.subTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.discountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.shippingCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.taxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.totalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.shippingAddressLine).NotEmpty().MaximumLength(300);
        RuleFor(x => x.shippingCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.shippingState).NotEmpty().MaximumLength(50);
        RuleFor(x => x.shippingZipCode).NotEmpty().MaximumLength(20);
    }
}