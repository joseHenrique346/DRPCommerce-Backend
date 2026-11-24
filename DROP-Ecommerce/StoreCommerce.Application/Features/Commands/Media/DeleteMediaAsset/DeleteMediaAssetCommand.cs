using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeleteMediaAssetCommand(long Id) : IRequest;
