// File: tests/ArchiXTest.ApiWeb/Test/RuntimeTests/DatabaseTests/ArchiXDatabaseErrorTests.cs

using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RuntimeTests.DatabaseTests
{
    /// <summary>
    /// Hatalı senaryolarda ArchiXDatabase davranışını test eder.
    /// </summary>
    public sealed class ArchiXDatabaseErrorTests
    {
        [Fact]
        public async Task CreateAsync_WithUnsupportedProvider_ShouldThrow()
        {
            ArchiXDatabase.Configure("UnknownProvider");

            await Assert.ThrowsAsync<NotSupportedException>(
                () => ArchiXDatabase.CreateAsync()
            );
        }

        [Fact]
        public async Task UpdateAsync_WithUnsupportedProvider_ShouldThrow()
        {
            ArchiXDatabase.Configure("UnknownProvider");

            await Assert.ThrowsAsync<NotSupportedException>(
                () => ArchiXDatabase.UpdateAsync()
            );
        }
    }
}
