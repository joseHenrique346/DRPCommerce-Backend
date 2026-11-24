using FluentValidation;
using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Customers;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Models;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchCustomersQueryValidator : AbstractValidator<SearchCustomersQuery>
{
    private static readonly string[] Sorts = ["newest", "oldest", "name", "total_spent_desc", "last_order_desc"];

    public SearchCustomersQueryValidator()
    {
        RuleFor(query => query.Term).MaximumLength(255);
        RuleFor(query => query.StateId)
            .Must(stateId => BaseStaticEntity.GetAll<State>().Any(state => state.Id == stateId)).When(query => query.StateId.HasValue)
            .WithMessage("{PropertyName} inválido.");
        RuleFor(query => query.CreatedTo).GreaterThanOrEqualTo(query => query.CreatedFrom)
            .When(query => query.CreatedFrom.HasValue && query.CreatedTo.HasValue).WithMessage("createdTo deve ser posterior a createdFrom.");
        RuleFor(query => query.Sort).Must(sort => sort is null || Sorts.Contains(sort))
            .WithMessage("sort deve ser newest, oldest, name, total_spent_desc ou last_order_desc.");
        RuleFor(query => query.Page).InclusiveBetween(1, 100000);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
