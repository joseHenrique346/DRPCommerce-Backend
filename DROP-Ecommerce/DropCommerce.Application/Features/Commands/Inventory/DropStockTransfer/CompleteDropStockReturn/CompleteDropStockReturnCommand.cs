using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using SharedCommerce.Application.Contracts.Drop;
using System.Text.Json;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed record CompleteDropStockReturnCommand(long Tenant, Guid OperationId) : IRequest<Result<bool>>;
