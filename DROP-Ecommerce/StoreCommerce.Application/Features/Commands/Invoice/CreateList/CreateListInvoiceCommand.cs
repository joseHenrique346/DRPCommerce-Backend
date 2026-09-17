using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListInvoiceCommand(List<CreateInvoiceCommand> commands) : IRequest<Result<List<Invoice>>> { }
