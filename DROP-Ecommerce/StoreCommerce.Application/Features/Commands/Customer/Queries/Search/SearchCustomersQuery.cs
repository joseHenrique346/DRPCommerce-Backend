using FluentValidation;
using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.Models;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Busca clientes por nome, e-mail ou telefone (term), situação, UF, período de cadastro e se já compraram.
/// Sort: newest (padrão), oldest, name, total_spent_desc, last_order_desc.
/// </summary>
public sealed record SearchCustomersQuery(
    string? Term = null, bool? IsActive = null, bool? IsVerified = null, bool? EmailConfirmed = null, long? StateId = null, bool? HasOrders = null,
    DateTime? CreatedFrom = null, DateTime? CreatedTo = null, string? Sort = null, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResponse<CustomerSearchItemResponse>>>;

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

public sealed class SearchCustomersQueryHandler(ICustomerRepository customers)
    : IRequestHandler<SearchCustomersQuery, Result<PagedResponse<CustomerSearchItemResponse>>>
{
    public async Task<Result<PagedResponse<CustomerSearchItemResponse>>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var (items, count) = await customers.SearchAsync(new CustomerSearchCriteria(request.Term, request.IsActive, request.IsVerified,
            request.EmailConfirmed, request.StateId, request.HasOrders, request.CreatedFrom, request.CreatedTo, request.Sort ?? "newest",
            request.Page, request.PageSize), cancellationToken);

        return Result<PagedResponse<CustomerSearchItemResponse>>.Success(new(items.Select(row => new CustomerSearchItemResponse(
            row.Id, row.FullName, row.Email, row.Phone, row.City, row.StateId, row.IsActive, row.IsVerified, row.EmailConfirmed,
            row.CompletedOrders, row.TotalSpent, row.LastOrderAt, row.CreatedAt)).ToList(), count, request.Page, request.PageSize));
    }
}
