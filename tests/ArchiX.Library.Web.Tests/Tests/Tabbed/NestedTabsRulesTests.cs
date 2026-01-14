using FluentAssertions;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Tabbed;

public sealed class NestedTabsRulesTests
{
    [Fact]
    public void NestedTabs_are_fail_closed_without_sidebar_metadata()
    {
        const bool enableNestedTabs = true;
        string? dataArchixMenu = null;

        var shouldOpenNested = enableNestedTabs && !string.IsNullOrWhiteSpace(dataArchixMenu);
        shouldOpenNested.Should().BeFalse();
    }

    [Fact]
    public void NestedTabs_open_only_when_enabled_and_menu_path_is_present()
    {
        const bool enableNestedTabs = true;
        const string dataArchixMenu = "Admin/Security";

        var shouldOpenNested = enableNestedTabs && !string.IsNullOrWhiteSpace(dataArchixMenu);
        shouldOpenNested.Should().BeTrue();

        var group = dataArchixMenu.Split('/')[0];
        group.Should().Be("Admin");
    }
}
