using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchCategoriesQueryValidator : AbstractValidator<SearchCategoriesQuery>
{
    public SearchCategoriesQueryValidator()
    {
        RuleFor(query => query.Name).MaximumLength(Category.NameMaxLength);
        RuleFor(query => query.ParentId).GreaterThan(0).When(query => query.ParentId.HasValue);
        RuleFor(query => query.Page).InclusiveBetween(1, 100000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.RootsOnly).Equal(false).When(query => query.ParentId.HasValue)
            .WithMessage("Não combine parentId com rootsOnly.");
    }
}
