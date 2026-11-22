using StoreCommerce.Domain.Interfaces.Identity;
using FluentValidation;
using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Customers;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Models;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

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
