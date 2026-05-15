using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetByIdDropOrderQuery(long id) : IRequest<Result<DropOrder>> { }
