using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListOrderItemCommand(List<long> ids) : IRequest<Result<bool>> { }
