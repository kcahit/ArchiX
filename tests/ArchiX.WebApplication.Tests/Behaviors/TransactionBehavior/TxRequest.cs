// File: tests/ArchiX.WebApplication.Tests/Behaviors/TransactionBehavior/TxRequest.cs
using ArchiX.WebApplication.Abstractions;

namespace ArchiX.WebApplication.Tests.Behaviors.TransactionBehavior
{
    /// <summary>
    /// TransactionBehavior testi için örnek istek.
    /// </summary>
    public sealed record TxRequest(string Value) : IRequest<string>;
}
