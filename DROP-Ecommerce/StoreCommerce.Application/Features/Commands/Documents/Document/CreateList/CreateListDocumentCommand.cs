using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListDocumentCommand(List<CreateDocumentCommand> commands) : IRequest<Result<List<Document>>> { }
