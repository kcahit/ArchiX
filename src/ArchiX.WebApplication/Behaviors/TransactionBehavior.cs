// File: src/ArchiX.WebApplication/Behaviors/TransactionBehavior.cs
using ArchiX.WebApplication.Abstractions.Delegates;
using ArchiX.WebApplication.Abstractions.Interfaces; // IRequest<>, IPipelineBehavior

using IUnitOfWork = ArchiX.Library.Abstractions.Persistence.IUnitOfWork; // Ambiguity fix

namespace ArchiX.WebApplication.Behaviors
{
    /// <summary>
    /// Handler çalıştıktan sonra <see cref="IUnitOfWork"/> ile değişiklikleri kalıcılaştırır.
    /// Başarısızlıkta kayıt yapılmaz, istisna üst katmana fırlatılır.
    /// </summary>
    /// <typeparam name="TRequest">İstek türü.</typeparam>
    /// <typeparam name="TResponse">Yanıt türü.</typeparam>
    public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
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
