using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateEnterpriseDomainCommand(long enterpriseId, string host, bool isPrimary, bool isActive) : IRequest<Result<EnterpriseDomain>>;
