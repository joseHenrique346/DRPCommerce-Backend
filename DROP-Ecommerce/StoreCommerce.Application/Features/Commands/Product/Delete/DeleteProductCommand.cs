using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity.Product;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteProductCommand(long id) : IRequest<Result<Product>> { }
