using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetByIdDropProductQuery(long id) : IRequest<Result<DropProduct>> { }
