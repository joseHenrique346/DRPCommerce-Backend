using MediatR;
using StoreCommerce.Application.Contracts.Store;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class GetPublicStoreInfoQuery : IRequest<Result<PublicStoreInfoResponse>> { }
