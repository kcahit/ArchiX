using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.TransactionBehavior
{
 public sealed record TxRequest(string Value) : IRequest<string>;
}
