using MediatR;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListInvoiceCommand(List<long> ids) : IRequest<Result<bool>> { }
