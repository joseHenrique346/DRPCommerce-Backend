using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListServiceCommand(List<UpdateServiceCommand> commands) : IRequest<Result<List<Service>>> { }
