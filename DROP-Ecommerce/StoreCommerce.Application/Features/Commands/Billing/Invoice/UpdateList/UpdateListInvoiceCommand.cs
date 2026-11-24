using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListInvoiceCommand(List<UpdateInvoiceCommand> commands) : IRequest<Result<List<Invoice>>> { }
