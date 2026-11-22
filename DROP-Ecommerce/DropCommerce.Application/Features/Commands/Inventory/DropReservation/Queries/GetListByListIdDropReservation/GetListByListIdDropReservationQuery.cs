using System.Collections.Generic;
using SharedCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetListByListIdDropReservationQuery(List<long> listId) : IRequest<Result<List<DropReservation>>> { }
