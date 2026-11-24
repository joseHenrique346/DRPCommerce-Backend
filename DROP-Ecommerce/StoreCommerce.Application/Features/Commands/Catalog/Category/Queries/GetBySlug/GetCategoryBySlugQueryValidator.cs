using FluentValidation;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCategoryBySlugQueryValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugQueryValidator()
    {
        RuleFor(query => query.Slug).NotEmpty().MaximumLength(Category.SlugMaxLength);
    }
}
