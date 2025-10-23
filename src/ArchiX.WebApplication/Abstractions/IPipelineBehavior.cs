// File: ArchiX.WebApplication/Abstractions/IPipelineBehavior.cs
namespace ArchiX.WebApplication.Abstractions
{
    /// <summary>
    /// İstek işleme hattında, gerçek handler'dan önce/sonra çalışabilen davranış adımı.
    /// </summary>
    /// <typeparam name="TRequest">İstek türü.</typeparam>
    /// <typeparam name="TResponse">Yanıt türü.</typeparam>
    public interface IPipelineBehavior<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Davranışı uygular ve sıradaki adımı çağırır.
        /// </summary>
        /// <param name="request">Gelen istek.</param>
        /// <param name="next">Zincirdeki bir sonraki adım.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken);
    }
}
