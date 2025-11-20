using ArchiX.Library.Web.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.TransactionBehavior
{
 public sealed record TxRequest(string Value) : IRequest<string>;
}
