using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetListByListIdUserQuery(List<long> ids) : IRequest<Result<List<User>>>;
