using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListProductCommand(List<UpdateProductCommand> commands) : IRequest<Result<List<Product>>> { }
