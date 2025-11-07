using ArchiX.Library.Web.Abstractions.Delegates;
using ArchiX.Library.Web.Abstractions.Interfaces;
using FluentValidation;
using FluentValidation.Results;

namespace ArchiX.Library.Web.Behaviors
{
 public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
 {
 private readonly IEnumerable<IValidator<TRequest>> _validators;
 public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators ?? Enumerable.Empty<IValidator<TRequest>>();
 public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
 {
 var failures = new List<ValidationFailure>();
 foreach (var v in _validators)
 {
 var r = await v.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
 if (!r.IsValid) failures.AddRange(r.Errors);
 }
 if (failures.Count >0) throw new ValidationException(failures);
 return await next(cancellationToken).ConfigureAwait(false);
 }
 }
}
