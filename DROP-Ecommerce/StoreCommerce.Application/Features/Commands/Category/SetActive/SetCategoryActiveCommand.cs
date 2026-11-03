using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SetCategoryActiveCommand(long Id, bool IsActive) : IRequest<Result<Category>>;
