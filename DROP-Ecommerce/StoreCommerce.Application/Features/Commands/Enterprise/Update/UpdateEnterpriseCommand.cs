using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateEnterpriseCommand(long id, string tradeName, string legalName, EnterpriseEmail email, EnterprisePhone phone, string addressLine, string city, string state, string zipCode, string country, bool isActive) : IRequest<Result<Enterprise>> { }
