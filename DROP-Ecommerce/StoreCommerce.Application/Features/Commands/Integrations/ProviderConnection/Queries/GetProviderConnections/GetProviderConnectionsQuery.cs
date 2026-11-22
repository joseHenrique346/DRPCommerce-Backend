using MediatR;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetProviderConnectionsQuery : IRequest<Result<IReadOnlyList<ProviderConnectionInfo>>>;
