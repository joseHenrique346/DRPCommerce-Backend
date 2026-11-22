using FluentValidation;
using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Customers;
using SharedCommerce.Application.Result;
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
