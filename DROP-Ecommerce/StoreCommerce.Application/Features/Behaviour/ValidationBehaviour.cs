using FluentValidation;
using MediatR;

namespace StoreCommerce.Application.Features;

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
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(v => v.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count > 0)
            {
                var errorMessages = failures.Select(f => f.ErrorMessage).ToList();

                return (TResponse)typeof(TResponse)
                    .GetMethod("FailureFromList")?
                    .Invoke(null, new object[] { errorMessages })!;
            }
        }

        return await next();
    }
}