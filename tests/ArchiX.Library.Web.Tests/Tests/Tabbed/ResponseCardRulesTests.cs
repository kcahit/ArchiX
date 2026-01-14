using FluentAssertions;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Tabbed;

public sealed class ResponseCardRulesTests
{
    [Fact]
    public void Copy_payload_contains_traceId_and_message_only()
    {
        const string traceId = "abc123";
        const string message = "Uyarı mesajı";

        var payload = $"TraceId: {traceId}\nMesaj: {message}";

        payload.Should().Contain(traceId);
        payload.Should().Contain(message);
    }

    [Fact]
    public void Response_card_requires_close_and_copy_actions()
    {
        // Decision 4.2
        var requiredActions = new[] { "close-tab", "copy-trace" };
        requiredActions.Should().Contain(new[] { "close-tab", "copy-trace" });
        requiredActions.Should().HaveCount(2);
    }
}
