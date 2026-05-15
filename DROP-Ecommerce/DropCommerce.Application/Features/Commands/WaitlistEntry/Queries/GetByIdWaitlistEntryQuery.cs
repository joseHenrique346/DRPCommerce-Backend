using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetByIdWaitlistEntryQuery(long id) : IRequest<Result<WaitlistEntry>> { }
