using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListSupplierCommand(List<CreateSupplierCommand> commands) : IRequest<Result<List<Supplier>>> { }
