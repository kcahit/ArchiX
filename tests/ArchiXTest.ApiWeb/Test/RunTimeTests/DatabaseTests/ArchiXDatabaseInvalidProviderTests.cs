// File: tests/ArchiXTest.ApiWeb/Test/RuntimeTests/DatabaseTests/ArchiXDatabaseInvalidProviderTests.cs

using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RuntimeTests.DatabaseTests
{
    /// <summary>
    /// Desteklenmeyen provider seçildiğinde hata fırlatıldığını test eder.
    /// </summary>
    public sealed class ArchiXDatabaseInvalidProviderTests
    {
        [Fact]
        public async Task Configure_WithInvalidProvider_ShouldThrow()
        {
            ArchiXDatabase.Configure("MongoDb");

            var ex = await Assert.ThrowsAsync<NotSupportedException>(
                () => ArchiXDatabase.CreateAsync()
            );

            Assert.Contains("Unsupported DB provider", ex.Message);
        }
    }
}
