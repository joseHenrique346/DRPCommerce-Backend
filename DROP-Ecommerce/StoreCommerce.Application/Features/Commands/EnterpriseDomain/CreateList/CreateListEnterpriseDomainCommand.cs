using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListEnterpriseDomainCommand(List<CreateEnterpriseDomainCommand> commands) : IRequest<Result<List<EnterpriseDomain>>>;
