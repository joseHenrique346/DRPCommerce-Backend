using System.Collections.Generic;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetAllFraudSignalQuery : IRequest<Result<List<FraudSignal>>> { }
