using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListEnterpriseDomainCommand(List<UpdateEnterpriseDomainCommand> commands) : IRequest<Result<List<EnterpriseDomain>>>;
