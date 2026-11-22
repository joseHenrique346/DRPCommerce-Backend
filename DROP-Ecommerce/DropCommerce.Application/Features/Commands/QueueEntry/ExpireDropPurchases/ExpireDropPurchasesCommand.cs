using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;
using SharedCommerce.Application.Contracts.Drop;
using System.Security.Cryptography;
using System.Text;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed record ExpireDropPurchasesCommand(long Tenant, long EventId) : IRequest<Result<bool>>;
