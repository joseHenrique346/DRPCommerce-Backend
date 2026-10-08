using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class GetAllPermissionQuery : IRequest<Result<List<Permission>>>;
public record class GetByIdPermissionQuery(long id) : IRequest<Result<Permission>>;
