using FluentValidation;
using StoreCommerce.Application.Contracts.Inventory;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CommitStockCommandValidator : AbstractValidator<CommitStockCommand>
{
    public CommitStockCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
    }
}
