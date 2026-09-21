using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateSupplierCommand(long enterpriseId, string companyName, string contactName, SupplierEmail email, SupplierPhone phone, string addressLine, string city, long stateId, string zipCode, string country, bool isActive) : IRequest<Result<Supplier>> { }
