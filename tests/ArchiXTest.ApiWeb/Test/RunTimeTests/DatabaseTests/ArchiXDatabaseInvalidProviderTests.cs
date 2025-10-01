// File: tests/ArchiXTest.ApiWeb/Test/RuntimeTests/DatabaseTests/ArchiXDatabaseInvalidProviderTests.cs
using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RuntimeTests.DatabaseTests;

public sealed class ArchiXDatabaseInvalidProviderTests
{
    [Fact]
    public void Configure_WithInvalidProvider_ShouldThrow()
    {
        // Arrange
        ArchiXDatabase.Configure("MongoDb");

        try
        {
            // Act + Assert
            var ex = Assert.Throws<NotSupportedException>(
                () => ArchiXDatabase.CreateAsync().GetAwaiter().GetResult());

            Assert.Contains("Unsupported DB provider", ex.Message);
        }
        finally
        {
            // Leakage önleme
            ArchiXDatabase.Configure("SqlServer");
        }
    }
}
