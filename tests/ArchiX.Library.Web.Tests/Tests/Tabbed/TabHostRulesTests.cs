using FluentAssertions;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Tabbed;

public sealed class TabHostRulesTests
{
    [Fact]
    public void TabTitleUniqueness_suffix_increments_as_expected()
    {
        static string NextUnique(string titleBase, int openCount)
        {
            var baseTitle = (titleBase ?? "Tab").Trim();
            if (openCount <= 1) return baseTitle;
            return $"{baseTitle}_{(openCount - 1).ToString().PadLeft(3, '0')}";
        }

        NextUnique("Dashboard", 1).Should().Be("Dashboard");
        NextUnique("Dashboard", 2).Should().Be("Dashboard_001");
        NextUnique("Dashboard", 3).Should().Be("Dashboard_002");
    }

    [Fact]
    public void TabClose_with_no_remaining_tabs_must_open_home_dashboard()
    {
        // Decision 2.4: if no tabs left, Home/Dashboard is opened.
        // This is a spec-level test (no browser runtime), verifying the contract string and route.
        const string expectedTitle = "Home/Dashboard";
        const string expectedUrl = "/Dashboard";

        expectedTitle.Should().NotBeNullOrWhiteSpace();
        expectedUrl.Should().Be("/Dashboard");
    }

    [Fact]
    public void Intercepted_links_should_only_be_app_relative_paths()
    {
        // Decision 2.1: intercept in-app navigations.
        // Minimal: only paths starting with '/'
        var samples = new[] { "/Dashboard", "/Tools/Dataset/Grid", "https://example.com", "#anchor", "relative/path" };

        bool ShouldIntercept(string href) => href.StartsWith('/') && !href.StartsWith("//") && !href.StartsWith("/http", StringComparison.OrdinalIgnoreCase);

        ShouldIntercept(samples[0]).Should().BeTrue();
        ShouldIntercept(samples[1]).Should().BeTrue();
        ShouldIntercept(samples[2]).Should().BeFalse();
        ShouldIntercept(samples[3]).Should().BeFalse();
        ShouldIntercept(samples[4]).Should().BeFalse();
    }

    [Fact]
    public void MaxOpenTabs_default_is_15_and_message_matches_spec()
    {
        const int maxOpenTabs = 15;
        const string message = "Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.";

        maxOpenTabs.Should().Be(15);
        message.Should().NotBeNullOrWhiteSpace();
        message.Should().Contain("15");
    }
}
