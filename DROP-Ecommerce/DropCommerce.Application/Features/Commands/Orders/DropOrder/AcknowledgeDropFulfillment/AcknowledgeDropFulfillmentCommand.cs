using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using SharedCommerce.Application.Contracts.Drop;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed record AcknowledgeDropFulfillmentCommand(long Tenant, long OrderId) : IRequest<Result<bool>>;
