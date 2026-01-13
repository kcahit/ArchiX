using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Grid;

public sealed class MultiInstanceOffcanvasIdTests
{
    [Fact]
    public void OffcanvasIds_Should_Be_InstanceScoped_By_TableId()
    {
        static string offcanvasId(string tableId) => $"archix-row-editor-{tableId}";

        Assert.NotEqual(offcanvasId("dsgrid-1"), offcanvasId("dsgrid-2"));
        Assert.Equal("archix-row-editor-dsgrid-1", offcanvasId("dsgrid-1"));
    }
}
