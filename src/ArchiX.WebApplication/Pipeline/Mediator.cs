// File: ArchiX.WebApplication/Pipeline/Mediator.cs
using ArchiX.WebApplication.Abstractions;

namespace ArchiX.WebApplication.Pipeline
{
    /// <summary>
    /// Kayıtlı handler ve davranış zinciri üzerinden istekleri işler.
    /// </summary>
    public sealed class Mediator : IMediator
    {
        private static readonly System.Reflection.MethodInfo SendCoreMethod =
            typeof(Mediator).GetMethod(nameof(SendCore),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var method = SendCoreMethod.MakeGenericMethod(request.GetType(), typeof(TResponse));
            return (Task<TResponse>)method.Invoke(this, [request, cancellationToken])!;
        }

        private Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
        {
            var handler =
                (IRequestHandler<TRequest, TResponse>?)_serviceProvider.GetService(typeof(IRequestHandler<TRequest, TResponse>))
                ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}.");

            var behaviors =
                (IEnumerable<IPipelineBehavior<TRequest, TResponse>>)
                (_serviceProvider.GetService(typeof(IEnumerable<IPipelineBehavior<TRequest, TResponse>>))
                 ?? Array.Empty<IPipelineBehavior<TRequest, TResponse>>());

            RequestHandlerDelegate<TResponse> terminal = _ => handler.HandleAsync(request, cancellationToken);

            foreach (var behavior in behaviors.Reverse())
            {
                var next = terminal;
                terminal = token => behavior.HandleAsync(request, next, token);
            }

            return terminal(cancellationToken);
        }
    }
}
