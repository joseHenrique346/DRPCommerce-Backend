using FluentValidation;
using MediatR;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errorMessages = failures.Select(f => f.ErrorMessage).ToList();

        var responseType = typeof(TResponse);
        var failureMethod = responseType.GetMethod("FailureFromList");

        if (failureMethod is null)
            throw new ValidationException(failures);

        return (TResponse)failureMethod.Invoke(null, new object[] { errorMessages })!;
    }
}