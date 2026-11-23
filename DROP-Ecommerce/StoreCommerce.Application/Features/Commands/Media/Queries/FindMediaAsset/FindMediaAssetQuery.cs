using MediatR;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record FindMediaAssetQuery(long Id) : IRequest<MediaAsset?>;
