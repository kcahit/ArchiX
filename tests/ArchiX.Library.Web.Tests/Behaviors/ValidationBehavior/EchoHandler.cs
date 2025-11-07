using ArchiX.WebApplication.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.ValidationBehavior
{
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
