using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListServiceCommand(List<CreateServiceCommand> commands) : IRequest<Result<List<Service>>> { }
