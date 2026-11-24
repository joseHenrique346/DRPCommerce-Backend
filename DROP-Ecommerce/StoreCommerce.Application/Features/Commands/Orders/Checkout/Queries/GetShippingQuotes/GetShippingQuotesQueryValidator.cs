using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetShippingQuotesQueryValidator : AbstractValidator<GetShippingQuotesQuery>
{
    public GetShippingQuotesQueryValidator()
    {
        RuleFor(Query => Query.Request).NotNull();
        RuleFor(Query => Query.Request.AddressId).GreaterThan(0).When(Query => Query.Request?.AddressId is not null);
        RuleFor(Query => Query.Request.UseProfileAddress).Equal(false).When(Query => Query.Request?.AddressId is not null);

        When(Query => Query.Request is { UseProfileAddress: false, AddressId: null }, () =>
        {
            RuleFor(Query => Query.Request.ShippingAddressLine).NotEmpty().MaximumLength(Order.ShippingAddressLineMaxLength);
            RuleFor(Query => Query.Request.ShippingCity).NotEmpty().MaximumLength(Order.ShippingCityMaxLength);
            RuleFor(Query => Query.Request.ShippingStateId).NotNull().GreaterThan(0);
            RuleFor(Query => Query.Request.ShippingZipCode).NotEmpty().MaximumLength(Order.ShippingZipCodeMaxLength);
        });
    }
}
