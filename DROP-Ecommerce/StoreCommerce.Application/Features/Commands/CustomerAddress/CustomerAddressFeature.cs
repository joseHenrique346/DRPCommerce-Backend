using FluentValidation;
using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

// ---- Vitrine: endereços do cliente autenticado

public sealed record GetMyAddressesQuery : IRequest<Result<List<CustomerAddressResponse>>>;

public sealed record AddMyAddressCommand(CustomerAddressRequest Request) : IRequest<Result<List<CustomerAddressResponse>>>;

public sealed record UpdateMyAddressCommand(long Id, CustomerAddressRequest Request) : IRequest<Result<List<CustomerAddressResponse>>>;

public sealed record SetMyDefaultAddressCommand(long Id) : IRequest<Result<List<CustomerAddressResponse>>>;

/// <summary>
/// Exclui o endereço. Se era o padrão, o endereço mais recente que restar passa a ser o padrão. Pedidos já feitos não mudam.
/// </summary>
public sealed record DeleteMyAddressCommand(long Id) : IRequest<Result<List<CustomerAddressResponse>>>;

// ---- Backoffice

public sealed record GetCustomerAddressesQuery(long CustomerId) : IRequest<Result<List<CustomerAddressResponse>>>;

public sealed class CustomerAddressRequestValidator : AbstractValidator<CustomerAddressRequest>
{
    public CustomerAddressRequestValidator()
    {
        RuleFor(request => request.Label).MaximumLength(CustomerAddress.LabelMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.RecipientName).MaximumLength(CustomerAddress.RecipientNameMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.AddressLine)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(CustomerAddress.AddressLineMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.City)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(CustomerAddress.CityMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.StateId)
            .Must(stateId => BaseStaticEntity.GetAll<State>().Any(state => state.Id == stateId)).WithMessage("{PropertyName} inválido.");
        RuleFor(request => request.ZipCode)
            .NotEmpty().WithMessage("{PropertyName} não pode ser vazio.")
            .MaximumLength(CustomerAddress.ZipCodeMaxLength).WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
        RuleFor(request => request.Country).MaximumLength(CustomerAddress.CountryMaxLength)
            .WithMessage("{PropertyName} não pode ter mais de {MaxLength} caracteres.");
    }
}

public sealed class AddMyAddressCommandValidator : AbstractValidator<AddMyAddressCommand>
{
    public AddMyAddressCommandValidator() => RuleFor(command => command.Request).NotNull().SetValidator(new CustomerAddressRequestValidator());
}

public sealed class UpdateMyAddressCommandValidator : AbstractValidator<UpdateMyAddressCommand>
{
    public UpdateMyAddressCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
        RuleFor(command => command.Request).NotNull().SetValidator(new CustomerAddressRequestValidator());
    }
}

public sealed class SetMyDefaultAddressCommandValidator : AbstractValidator<SetMyDefaultAddressCommand>
{
    public SetMyDefaultAddressCommandValidator() => RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
}

public sealed class DeleteMyAddressCommandValidator : AbstractValidator<DeleteMyAddressCommand>
{
    public DeleteMyAddressCommandValidator() => RuleFor(command => command.Id).GreaterThan(0).WithMessage("{PropertyName} deve ser maior que zero.");
}

public sealed class GetMyAddressesQueryHandler(CustomerAddressService service)
    : WorkflowRequestHandler<GetMyAddressesQuery, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(GetMyAddressesQuery request, CancellationToken ct) => service.ListAsync(ct);
}

public sealed class AddMyAddressCommandHandler(CustomerAddressService service)
    : WorkflowRequestHandler<AddMyAddressCommand, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(AddMyAddressCommand request, CancellationToken ct) => service.AddAsync(request.Request, ct);
}

public sealed class UpdateMyAddressCommandHandler(CustomerAddressService service)
    : WorkflowRequestHandler<UpdateMyAddressCommand, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(UpdateMyAddressCommand request, CancellationToken ct) =>
        service.UpdateAsync(request.Id, request.Request, ct);
}

public sealed class SetMyDefaultAddressCommandHandler(CustomerAddressService service)
    : WorkflowRequestHandler<SetMyDefaultAddressCommand, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(SetMyDefaultAddressCommand request, CancellationToken ct) =>
        service.SetDefaultAsync(request.Id, ct);
}

public sealed class DeleteMyAddressCommandHandler(CustomerAddressService service)
    : WorkflowRequestHandler<DeleteMyAddressCommand, List<CustomerAddressResponse>>
{
    protected override Task<List<CustomerAddressResponse>> ExecuteAsync(DeleteMyAddressCommand request, CancellationToken ct) =>
        service.DeleteAsync(request.Id, ct);
}

public sealed class GetCustomerAddressesQueryHandler(ICustomerAddressRepository addresses, IRepository<Customer> customers, ITenantProvider tenant)
    : WorkflowRequestHandler<GetCustomerAddressesQuery, List<CustomerAddressResponse>>
{
    protected override async Task<List<CustomerAddressResponse>> ExecuteAsync(GetCustomerAddressesQuery request, CancellationToken ct)
    {
        if (!await customers.ExistsAsync(customer => customer.Id == request.CustomerId, ct))
            throw new SecurityFailure(404, "Customer.NotFound", "Cliente não encontrado.");

        return (await addresses.ListForCustomerAsync(tenant.GetEnterpriseId(), request.CustomerId, tracking: false, ct))
            .Select(CustomerAddressResponse.FromEntity).ToList();
    }
}
