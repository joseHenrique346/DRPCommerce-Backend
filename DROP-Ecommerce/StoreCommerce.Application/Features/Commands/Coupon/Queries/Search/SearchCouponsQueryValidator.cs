using FluentValidation;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchCouponsQueryValidator : AbstractValidator<SearchCouponsQuery>
{
    public SearchCouponsQueryValidator()
    {
        RuleFor(query => query.Code).MaximumLength(Coupon.CodeMaxLength);
        RuleFor(query => query.TypeId)
            .Must(typeId => BaseStaticEntity.GetAll<CouponType>().Any(type => type.Id == typeId)).When(query => query.TypeId.HasValue)
            .WithMessage("{PropertyName} inválido.");
        RuleFor(query => query.Availability).IsInEnum().When(query => query.Availability.HasValue);
        RuleFor(query => query.Page).InclusiveBetween(1, 100000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
