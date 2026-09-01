using FluentValidation;
using MediatR;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Behaviors;

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
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();

            // Attempt to return ApiResponse with errors if TResponse is ApiResponse<T>
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                var failureMethod = responseType.GetMethod("FailureResponse",
                    new[] { typeof(string), typeof(List<string>) });

                if (failureMethod is not null)
                {
                    var response = failureMethod.Invoke(null,
                        new object[] { "Validation failed.", errors });
                    return (TResponse)response!;
                }
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
