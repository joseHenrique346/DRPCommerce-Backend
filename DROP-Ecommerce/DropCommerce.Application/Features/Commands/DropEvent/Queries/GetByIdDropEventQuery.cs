using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetByIdDropEventQuery(long id) : IRequest<Result<DropEvent>> { }
