using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Identity;

public interface IIdentityRequest<TResponse> : IRequest<Result<TResponse>>, IIdentityOperation
{
}
