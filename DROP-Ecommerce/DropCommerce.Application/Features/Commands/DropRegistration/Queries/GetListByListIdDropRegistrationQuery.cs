using System.Collections.Generic;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetListByListIdDropRegistrationQuery(List<long> listId) : IRequest<Result<List<DropRegistration>>> { }
