// File: tests/ArchiX.WebApplication.Tests/Behaviors/TransactionBehavior/TxHandlerThrowing.cs
using ArchiX.WebApplication.Abstractions;

namespace ArchiX.WebApplication.Tests.Behaviors.TransactionBehavior
{
    /// <summary>
    /// Hata fırlatan handler: TransactionBehavior'ın commit etmeyeceğini doğrulamak için kullanılır.
    /// </summary>
    public sealed class TxHandlerThrowing : IRequestHandler<TxRequest, string>
    {
        public Task<string> HandleAsync(TxRequest request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("tx-fail");
    }
}
