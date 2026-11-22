using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Contracts.Drop;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed record PostponePlatformSubscriptionCommand(long Tenant, long SubscriptionId) : IIdentityRequest<bool>;
