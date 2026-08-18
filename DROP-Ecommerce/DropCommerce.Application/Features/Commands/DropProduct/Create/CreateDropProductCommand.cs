using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateDropProductCommand(long dropEventId, long productId, string sku, int unitsAllocated, int unitsSold, int maxPerCustomer, decimal price, bool isActive) : IRequest<Result<DropProduct>> { }
