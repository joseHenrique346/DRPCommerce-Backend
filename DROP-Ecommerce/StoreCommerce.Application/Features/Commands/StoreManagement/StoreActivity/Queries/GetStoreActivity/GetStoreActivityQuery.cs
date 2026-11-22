using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Contracts.Common;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetStoreActivityQuery(StoreActivityQueryRequest Request)
    : IRequest<Result<PagedResponse<StoreActivityResponse>>>;
