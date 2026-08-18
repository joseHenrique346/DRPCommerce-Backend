using FluentValidation;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropEventCommandValidator : AbstractValidator<UpdateDropEventCommand>
{
    public UpdateDropEventCommandValidator()
    {
        RuleFor(x => x.id).GreaterThan(0);
        RuleFor(x => x.enterpriseId).GreaterThan(0);
        RuleFor(x => x.productId).GreaterThan(0);
        RuleFor(x => x.name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.slug).NotEmpty().MaximumLength(200);
        RuleFor(x => x.description).NotEmpty();
        RuleFor(x => x.coverImageUrl).NotEmpty();
        RuleFor(x => x.bannerImageUrl).NotEmpty();
        RuleFor(x => x.statusId).GreaterThan(0);
        RuleFor(x => x.totalUnitsAvailable).GreaterThan(0);
        RuleFor(x => x.unitsReserved).GreaterThanOrEqualTo(0);
        RuleFor(x => x.unitsSold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.price).GreaterThan(0);
        RuleFor(x => x.registrationStartsAt).NotEqual(default(DateTime));
        RuleFor(x => x.registrationEndsAt).GreaterThan(x => x.registrationStartsAt);
        RuleFor(x => x.dropStartsAt).NotEqual(default(DateTime));
        RuleFor(x => x.dropEndsAt).GreaterThan(x => x.dropStartsAt);
        RuleFor(x => x.queueOpensAt).NotEqual(default(DateTime));
    }
}