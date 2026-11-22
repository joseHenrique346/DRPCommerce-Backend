using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListSupplierCommand(List<UpdateSupplierCommand> commands) : IRequest<Result<List<Supplier>>> { }
