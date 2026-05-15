using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class CreateDropEventCommand(long enterpriseId, long productId, string name, string slug, string description, string coverImageUrl, string bannerImageUrl, long statusId, int totalUnitsAvailable, int unitsReserved, int unitsSold, decimal price, bool requiresRegistration, bool isPublic, DateTime registrationStartsAt, DateTime registrationEndsAt, DateTime queueOpensAt, DateTime dropStartsAt, DateTime dropEndsAt) : IRequest<Result<DropEvent>> { }
