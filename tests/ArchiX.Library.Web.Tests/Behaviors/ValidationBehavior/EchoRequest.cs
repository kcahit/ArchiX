using ArchiX.Library.Web.Abstractions.Interfaces;

namespace ArchiX.Library.Web.Tests.Behaviors.ValidationBehavior
{
 public sealed class EchoRequest : IRequest<string>
 {
 public string? Name { get; init; }
 }
}
