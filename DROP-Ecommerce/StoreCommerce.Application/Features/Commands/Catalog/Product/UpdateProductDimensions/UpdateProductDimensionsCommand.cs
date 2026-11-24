using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateProductDimensionsCommand(long Id, decimal Weight, decimal Height, decimal Width, decimal Length) : IRequest<Result<Product>>;
