using MediatR;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record FindStorefrontMediaAssetQuery(Guid PublicId) : IRequest<MediaAsset?>;
