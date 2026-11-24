using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
{
    public ReleaseStockCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}
