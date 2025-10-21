// File: ArchiX.WebApplication/Abstractions/IRequest.cs
namespace ArchiX.WebApplication.Abstractions
{
    /// <summary>
    /// Represents a request that produces a response handled by a corresponding handler.
    /// </summary>
    /// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
    public interface IRequest<out TResponse>
    {
    }
}
