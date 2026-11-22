using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record PostponeDropSalePaymentCommand(long Tenant, Guid OperationId) : IIdentityRequest<bool>;
