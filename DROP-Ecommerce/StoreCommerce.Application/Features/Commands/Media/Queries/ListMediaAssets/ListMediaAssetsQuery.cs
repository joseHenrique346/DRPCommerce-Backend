using StoreCommerce.Application.Media;
using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListMediaAssetsQuery() : IRequest<IReadOnlyList<MediaAssetSummary>>;
