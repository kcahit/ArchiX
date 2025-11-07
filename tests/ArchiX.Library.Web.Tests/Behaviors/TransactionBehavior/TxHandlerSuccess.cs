using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.TransactionBehavior
{
 /// <summary>
 /// Successful handler used to verify TransactionBehavior commits.
 /// </summary>
 public sealed class TxHandlerSuccess : IRequestHandler<TxRequest, string>
 {
 public Task<string> HandleAsync(TxRequest request, CancellationToken cancellationToken)
 {
 return Task.FromResult("ok");
 }
 }
}
