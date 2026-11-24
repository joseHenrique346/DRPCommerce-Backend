using FluentValidation;
using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Identity;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Exclui o endereço. Se era o padrão, o endereço mais recente que restar passa a ser o padrão. Pedidos já feitos não mudam.
/// </summary>
public sealed record DeleteMyAddressCommand(long Id) : IRequest<Result<List<CustomerAddressResponse>>>;
