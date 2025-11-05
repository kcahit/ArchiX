// File: src/ArchiX.WebApplication/Behaviors/TransactionBehavior.cs
using ArchiX.WebApplication.Abstractions.Delegates;
using ArchiX.Library.Abstractions.Persistence;       // IUnitOfWork

namespace ArchiX.WebApplication.Behaviors
{
    /// <summary>
    /// Handler çalıştıktan sonra <see cref="IUnitOfWork"/> ile değişiklikleri kalıcılaştırır.
    /// Başarısızlıkta kayıt yapılmaz, istisna üst katmana fırlatılır.
    /// </summary>
    /// <typeparam name="TRequest">İstek türü.</typeparam>
    /// <typeparam name="TResponse">Yanıt türü.</typeparam>
    public sealed class TransactionBehavior<TRequest, TResponse>
        : ArchiX.WebApplication.Abstractions.Interfaces.IPipelineBehavior<TRequest, TResponse>
        where TRequest : ArchiX.WebApplication.Abstractions.Interfaces.IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Yeni örnek oluşturur.
        /// </summary>
        public TransactionBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <inheritdoc />
        public async Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var response = await next(cancellationToken).ConfigureAwait(false);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return response;
        }
    }
}
