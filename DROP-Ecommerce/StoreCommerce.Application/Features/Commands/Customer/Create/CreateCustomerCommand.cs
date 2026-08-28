using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateCustomerCommand(long enterpriseId, string fullName, string passwordHash, string addressLine, string city, string state, string zipCode, string country, string gender, DateTime dateOfBirth, bool isVerified, bool isActive) : IRequest<Result<Customer>> { }
