using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Abstractions.Connections;
using ArchiX.Library.Runtime.Connections;
using Xunit;

namespace ArchiX.Library.Tests.Tests.ConnectionTests;

public sealed class ConnectionStringBuilderServiceTests
{
    private sealed class FakeProfiles : IConnectionProfileProvider
    {
        private readonly ConnectionProfile _profile;
        public FakeProfiles(ConnectionProfile profile) => _profile = profile;
        public ValueTask<ConnectionProfile> GetProfileAsync(string connectionName, CancellationToken ct = default) => ValueTask.FromResult(_profile);
    }

    private sealed class FakeSecrets : ISecretResolver
    {
        private readonly string? _val;
        public FakeSecrets(string? val) => _val = val;
        public ValueTask<string?> TryResolveAsync(string secretRef, CancellationToken ct = default) => ValueTask.FromResult(_val);
    }

    private sealed class FakePolicy : IConnectionPolicyEvaluator
    {
        private readonly string _result;
        public FakePolicy(string result) => _result = result;
        public ConnectionPolicyResult Evaluate(string connectionString) => new("Enforce", _result, _result == "Blocked" ? "X" : null, "localhost");
    }

    [Fact]
    public async Task BuildAndValidateAsync_FailsClosed_WhenPasswordRefCannotBeResolved()
    {
        var svc = new ConnectionStringBuilderService(
            new FakeProfiles(new ConnectionProfile("SqlServer", "localhost", "Db", "SqlLogin", "sa", "ENV:XXX", Encrypt: true, TrustServerCertificate: false)),
            new FakeSecrets(null),
            new FakePolicy("Allowed"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.BuildAndValidateAsync("Any").AsTask());
    }

    [Fact]
    public async Task BuildAndValidateAsync_Blocks_WhenPolicyBlocks()
    {
        var svc = new ConnectionStringBuilderService(
            new FakeProfiles(new ConnectionProfile("SqlServer", "localhost", "Db", "SqlLogin", "sa", "ENV:XXX", Encrypt: true, TrustServerCertificate: false)),
            new FakeSecrets("pwd"),
            new FakePolicy("Blocked"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.BuildAndValidateAsync("Any").AsTask());
    }
}
