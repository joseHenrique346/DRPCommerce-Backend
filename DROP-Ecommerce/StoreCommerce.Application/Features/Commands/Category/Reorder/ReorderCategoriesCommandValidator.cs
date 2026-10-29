using FluentValidation;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReorderCategoriesCommandValidator : AbstractValidator<ReorderCategoriesCommand>
{
    public ReorderCategoriesCommandValidator()
    {
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items).NotNull().ChildRules(item =>
        {
            item.RuleFor(value => value.Id).GreaterThan(0);
            item.RuleFor(value => value.DisplayOrder).GreaterThanOrEqualTo(0);
        });
    }
}
