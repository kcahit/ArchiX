// File: ArchiX.WebApplication/Abstractions/IMediator.cs
namespace ArchiX.WebApplication.Abstractions.Interfaces
{
    /// <summary>
    /// Uygulama içi istekleri uygun handler ve davranış hattı üzerinden işler.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// İsteği işler ve yanıt döner.
        /// </summary>
        /// <typeparam name="TResponse">Yanıt türü.</typeparam>
        /// <param name="request">İstek nesnesi.</param>
        /// <param name="cancellationToken">İptal belirteci.</param>
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
