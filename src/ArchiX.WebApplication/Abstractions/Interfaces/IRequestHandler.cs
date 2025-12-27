// File: ArchiX.WebApplication/Abstractions/IRequestHandler.cs
namespace ArchiX.WebApplication.Abstractions.Interfaces
{
    /// <summary>
    /// Handles a given <see cref="IRequest{TResponse}"/> and produces a response.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Processes the request and returns a response.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The response for the request.</returns>
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
