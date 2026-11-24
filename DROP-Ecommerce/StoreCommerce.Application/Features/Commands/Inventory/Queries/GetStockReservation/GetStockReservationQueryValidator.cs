using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetStockReservationQueryValidator : AbstractValidator<GetStockReservationQuery>
{
    public GetStockReservationQueryValidator()
    {
        RuleFor(Query => Query.Id).GreaterThan(0);
    }
}
