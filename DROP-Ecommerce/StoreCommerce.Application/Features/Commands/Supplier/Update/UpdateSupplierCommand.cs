using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateSupplierCommand(long id, long enterpriseId, string companyName, string contactName, string addressLine, string city, string state, string zipCode, string country, bool isActive) : IRequest<Result<Supplier>> { }
