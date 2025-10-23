// File: ArchiX.WebApplication/Abstractions/RequestHandlerDelegate.cs
namespace ArchiX.WebApplication.Abstractions
{
    /// <summary>
    /// Pipeline'da bir sonraki adımı temsil eden temsilci.
    /// </summary>
    /// <typeparam name="TResponse">Yanıt türü.</typeparam>
    /// <param name="cancellationToken">İptal belirteci.</param>
    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);
}
