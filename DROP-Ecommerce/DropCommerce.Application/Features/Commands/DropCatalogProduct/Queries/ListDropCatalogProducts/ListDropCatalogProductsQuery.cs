using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public sealed record ListDropCatalogProductsQuery(long? AfterId) : IRequest<Result<IReadOnlyList<DropCatalogProduct>>>;
