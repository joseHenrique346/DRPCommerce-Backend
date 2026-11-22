using StoreCommerce.Application.Interfaces.Identity;
using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Contracts.Drop;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListDuePlatformSubscriptionsQuery() : IIdentityRequest<List<PlatformBillingKey>>;
