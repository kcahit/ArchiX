// File: tests/ArchiX.WebApplication.Tests/Behaviors/ValidationBehavior/EchoRequest.cs
using ArchiX.WebApplication.Abstractions;

namespace ArchiX.WebApplication.Tests.Behaviors.ValidationBehavior
{
    /// <summary>
    /// ValidationBehavior testleri için örnek istek.
    /// </summary>
    public sealed class EchoRequest : IRequest<string>
    {
        /// <summary>Ad.</summary>
        public string? Name { get; init; }
    }
}
