using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.TransactionBehavior
{
 /// <summary>
 /// Handler that throws, used to verify TransactionBehavior does not commit.
 /// </summary>
 public sealed class TxHandlerThrowing : IRequestHandler<TxRequest, string>
 {
 public Task<string> HandleAsync(TxRequest request, CancellationToken cancellationToken)
 => throw new InvalidOperationException("tx-fail");
 }
}
