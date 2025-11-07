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
 if (!authorized) throw new UnauthorizedAccessException();
 return await next(cancellationToken).ConfigureAwait(false);
 }
 }
}
