using System.Reflection;
using ArchiX.Library.Web.Abstractions.Delegates;
using ArchiX.Library.Web.Abstractions.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Pipeline
{
 public sealed class Mediator : IMediator
 {
 private readonly IServiceProvider _sp;
 public Mediator(IServiceProvider sp) => _sp = sp;
 
 Task<TResponse> IMediator.SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
 {
 var method = typeof(Mediator).GetMethod(nameof(SendCore), BindingFlags.NonPublic | BindingFlags.Instance)!;
 var generic = method.MakeGenericMethod(request.GetType(), typeof(TResponse));
 return (Task<TResponse>)generic.Invoke(this, new object[] { request, cancellationToken })!;
 }
 
 public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
 where TRequest : IRequest<TResponse>
 => SendCore<TRequest, TResponse>(request, cancellationToken);
 
 private async Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken ct) where TRequest : IRequest<TResponse>
 {
 var handlers = _sp.GetServices<ArchiX.Library.Web.Abstractions.Interfaces.IRequestHandler<TRequest, TResponse>>().ToList();
 if (handlers.Count ==0) throw new InvalidOperationException($"No handler for {typeof(TRequest).Name}");
 if (handlers.Count >1) throw new InvalidOperationException($"Multiple handlers for {typeof(TRequest).Name}");
 var handler = handlers[0];
 var behaviors = _sp.GetServices<ArchiX.Library.Web.Abstractions.Interfaces.IPipelineBehavior<TRequest, TResponse>>().Reverse().ToList();
 ArchiX.Library.Web.Abstractions.Delegates.RequestHandlerDelegate<TResponse> next = t => handler.HandleAsync(request, t);
 foreach (var b in behaviors) { var current = next; next = t => b.HandleAsync(request, current, t); }
 return await next(ct).ConfigureAwait(false);
 }
 }
}
