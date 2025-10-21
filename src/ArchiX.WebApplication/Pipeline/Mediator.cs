// File: src/ArchiX.WebApplication/Pipeline/Mediator.cs
using System.Reflection;

using ArchiX.WebApplication.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Pipeline
{
    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _sp;
        public Mediator(IServiceProvider sp) => _sp = sp;

        // IMediator: IRequest<TResponse> imzası → gerçek TRequest türüyle SendCore<TReq,TRes>
        Task<TResponse> IMediator.SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            var method = typeof(Mediator).GetMethod(nameof(SendCore), BindingFlags.NonPublic | BindingFlags.Instance)!;
            var generic = method.MakeGenericMethod(request.GetType(), typeof(TResponse));
            return (Task<TResponse>)generic.Invoke(this, new object?[] { request, cancellationToken })!;
        }

        // IMediator: TRequest,TResponse imzası
        public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse>
            => SendCore<TRequest, TResponse>(request, cancellationToken);

        // Gerçek yürütme
        private async Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken ct)
            where TRequest : IRequest<TResponse>
        {
            var handlers = _sp.GetServices<IRequestHandler<TRequest, TResponse>>().ToList();
            if (handlers.Count == 0)
                throw new InvalidOperationException($"No handler for {typeof(TRequest).Name}");
            if (handlers.Count > 1)
                throw new InvalidOperationException($"Multiple handlers for {typeof(TRequest).Name}");

            var handler = handlers[0];

            var behaviors = _sp.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse().ToList();
            RequestHandlerDelegate<TResponse> next = t => handler.HandleAsync(request, t);
            foreach (var b in behaviors)
            {
                var current = next;
                next = t => b.HandleAsync(request, current, t);
            }
            return await next(ct).ConfigureAwait(false);
        }
    }
}
