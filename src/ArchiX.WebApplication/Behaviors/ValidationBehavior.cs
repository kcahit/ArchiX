// File: ArchiX.WebApplication/Behaviors/ValidationBehavior.cs
using ArchiX.WebApplication.Abstractions.Delegates;
using ArchiX.WebApplication.Abstractions.Interfaces;

using FluentValidation;
using FluentValidation.Results;

namespace ArchiX.WebApplication.Behaviors
{
    /// <summary>
    /// FluentValidation doğrulamasını çalıştırır; hatalar varsa istisna atar, yoksa sıradaki adıma geçer.
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly System.Collections.Generic.IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(System.Collections.Generic.IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators ?? [];
        }

        public async Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            System.Collections.Generic.List<ValidationFailure> failures = [];

            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    failures.AddRange(result.Errors);
                }
            }

            if (failures.Count > 0)
            {
                throw new FluentValidation.ValidationException(failures);
            }

            return await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
