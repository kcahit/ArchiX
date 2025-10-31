// File: tests/ArchiXTest.ApiWeb/Test/RuntimeTests/DatabaseTests/ArchiXDatabaseInvalidProviderTests.cs
using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RuntimeTests.DatabaseTests;

public sealed class ArchiXDatabaseInvalidProviderTests
{
    [Fact]
    public void Configure_WithInvalidProvider_ShouldThrow()
    {
        var ex = Assert.Throws<NotSupportedException>(() => ArchiXDatabase.Configure("MongoDb"));
        Assert.Contains("Unsupported DB provider", ex.Message);

        // Güvenli reset (başka testler etkilenmesin)
        ArchiXDatabase.Configure("SqlServer");
    }

    [Fact]
    public void TryConfigure_WithInvalidProvider_ShouldReturnFalse()
    {
        var ok = ArchiXDatabase.TryConfigure("MongoDb");
        Assert.False(ok);
    }
}
