using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;
using SharedCommerce.Application.Contracts.Drop;
using System.Text.Json;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed record ImportDropStockCommand(DropStockExport Export) : IRequest<Result<DropStockImportReceipt>>;
