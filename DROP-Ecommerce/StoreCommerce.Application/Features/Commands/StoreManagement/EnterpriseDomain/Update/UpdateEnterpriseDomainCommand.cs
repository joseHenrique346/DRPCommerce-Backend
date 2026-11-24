using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateEnterpriseDomainCommand(long id, string host, bool isPrimary, bool isActive) : IRequest<Result<EnterpriseDomain>>;
