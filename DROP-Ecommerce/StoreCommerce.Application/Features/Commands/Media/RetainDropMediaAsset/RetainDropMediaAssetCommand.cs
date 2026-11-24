using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RetainDropMediaAssetCommand(long Tenant, long Id) : IRequest<MediaAssetReference?>;
