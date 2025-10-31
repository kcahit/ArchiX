// File: tests/ArchiX.WebApplication.Tests/Behaviors/TransactionBehavior/TxHandlerSuccess.cs
using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.WebApplication.Tests.Behaviors.TransactionBehavior
{
    /// <summary>
    /// Başarılı handler: değeri aynen döner.
    /// </summary>
    public sealed class TxHandlerSuccess : IRequestHandler<TxRequest, string>
    {
        public Task<string> HandleAsync(TxRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }
}
