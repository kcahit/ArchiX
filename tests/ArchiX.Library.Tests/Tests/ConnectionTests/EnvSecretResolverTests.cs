using ArchiX.Library.Runtime.Connections;
using Xunit;

namespace ArchiX.Library.Tests.Tests.ConnectionTests;

public sealed class EnvSecretResolverTests
{
    [Fact]
    public async Task TryResolveAsync_ReturnsNull_WhenNotEnvRef()
    {
        var r = new EnvSecretResolver();
        var v = await r.TryResolveAsync("KV:abc");
        Assert.Null(v);
    }

    [Fact]
    public async Task TryResolveAsync_ReturnsValue_WhenEnvVarExists()
    {
        Environment.SetEnvironmentVariable("TEST_ENV_SECRET", "s3cr3t");
        try
        {
            var r = new EnvSecretResolver();
            var v = await r.TryResolveAsync("ENV:TEST_ENV_SECRET");
            Assert.Equal("s3cr3t", v);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_ENV_SECRET", null);
        }
    }
}
