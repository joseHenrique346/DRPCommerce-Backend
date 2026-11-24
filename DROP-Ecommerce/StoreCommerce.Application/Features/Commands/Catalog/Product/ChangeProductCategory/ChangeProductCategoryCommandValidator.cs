using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ChangeProductCategoryCommandValidator : AbstractValidator<ChangeProductCategoryCommand>
{
    public ChangeProductCategoryCommandValidator()
    {
        RuleFor(Command => Command.Id).GreaterThan(0);
        RuleFor(Command => Command.CategoryId).GreaterThan(0);
        RuleFor(Command => Command.SupplierId).GreaterThan(0).When(Command => Command.SupplierId.HasValue);
    }
}
