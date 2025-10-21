// File: tests/ArchiX.WebApplication.Tests/Behaviors/ValidationBehavior/EchoHandler.cs
using ArchiX.WebApplication.Abstractions;

namespace ArchiX.WebApplication.Tests.Behaviors.ValidationBehavior
{
    /// <summary>
    /// Örnek handler. Geçerli istekte ad ile selamlama döner.
    /// </summary>
    public sealed class EchoHandler : IRequestHandler<EchoRequest, string>
    {
        public Task<string> HandleAsync(EchoRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = request?.Name;
            return Task.FromResult(string.IsNullOrWhiteSpace(name) ? "Hello!" : $"Hello, {name.Trim()}!");
        }
    }
}
