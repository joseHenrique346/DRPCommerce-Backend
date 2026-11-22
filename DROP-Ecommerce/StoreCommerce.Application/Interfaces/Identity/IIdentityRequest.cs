using StoreCommerce.Application.Identity;
using MediatR;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Interfaces.Identity;

public interface IIdentityRequest<TResponse> : IRequest<Result<TResponse>>, IIdentityOperation
{
}
