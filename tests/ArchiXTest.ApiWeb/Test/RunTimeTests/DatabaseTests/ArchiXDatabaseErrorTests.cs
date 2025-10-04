// File: tests/ArchiXTest.ApiWeb/Test/RuntimeTests/DatabaseTests/ArchiXDatabaseErrorTests.cs
using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RuntimeTests.DatabaseTests
{
    /// <summary>Hatalı sağlayıcı durumlarında ArchiXDatabase davranışını test eder.</summary>
    public sealed class ArchiXDatabaseErrorTests
    {
        [Fact]
        public async Task CreateAsync_WithUnsupportedProvider_ShouldThrow()
        {
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                ArchiXDatabase.Configure("UnknownProvider"); // senkron fırlatır → async lambda Task'ı faulted olur
                await ArchiXDatabase.CreateAsync();
            });
        }

        [Fact]
        public async Task UpdateAsync_WithUnsupportedProvider_ShouldThrow()
        {
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
            {
                ArchiXDatabase.Configure("UnknownProvider"); // senkron fırlatır → async lambda Task'ı faulted olur
                await ArchiXDatabase.UpdateAsync();
            });
        }
    }
}
