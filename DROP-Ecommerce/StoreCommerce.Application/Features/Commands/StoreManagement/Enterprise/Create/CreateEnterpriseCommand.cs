using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateEnterpriseCommand(string tradeName, string legalName, EnterpriseEmail email, EnterprisePhone phone, string addressLine, string city, string state, string zipCode, string country, bool isActive) : IRequest<Result<Enterprise>> { }
