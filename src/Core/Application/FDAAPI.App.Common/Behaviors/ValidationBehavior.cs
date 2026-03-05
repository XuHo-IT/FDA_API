using FluentValidation;
using MediatR;

namespace FDAAPI.App.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior that automatically validates all requests using FluentValidation.
/// This runs before the actual handler and throws ValidationException if validation fails.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators registered for this request type, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If there are validation failures, throw ValidationException
        // This will be caught by FastEndpoints and converted to 400 Bad Request
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Validation passed, proceed to the handler
        return await next();
    }
}

