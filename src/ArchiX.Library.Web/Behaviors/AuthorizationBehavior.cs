using System.Reflection;
using ArchiX.Library.Web.Abstractions.Authorizations;
using ArchiX.Library.Web.Abstractions.Delegates;
using ArchiX.Library.Web.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Behaviors
{
 public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
 where TRequest : IRequest<TResponse>
 {
 private readonly IAuthorizationService _authorizationService;
 public AuthorizationBehavior(IAuthorizationService authorizationService) => _authorizationService = authorizationService;

 public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
 {
 var attr = typeof(TRequest).GetCustomAttribute<AuthorizeAttribute>(inherit: true);
 if (attr is null || attr.Policies.Count ==0) return await next(cancellationToken).ConfigureAwait(false);
 var authorized = await _authorizationService.AuthorizeAsync(attr.Policies, attr.RequireAll, cancellationToken).ConfigureAwait(false);
 if (!authorized)
 {
 var reqName = typeof(TRequest).Name;
 var policies = string.Join(", ", attr.Policies);
 throw new UnauthorizedAccessException($"Attempted to perform an unauthorized operation '{reqName}' requiring policies [{policies}] (RequireAll={attr.RequireAll}).");
 }
 return await next(cancellationToken).ConfigureAwait(false);
 }
 }
}
