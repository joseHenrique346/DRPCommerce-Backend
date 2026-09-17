using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListDocumentCommand(List<UpdateDocumentCommand> commands) : IRequest<Result<List<Document>>> { }
