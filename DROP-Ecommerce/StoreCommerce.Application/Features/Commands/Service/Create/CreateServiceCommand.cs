using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity.Service;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateServiceCommand(long enterpriseId, long categoryId, string name, string description, decimal price, int durationMinutes, bool isActive) : IRequest<Result<Service>> { }
