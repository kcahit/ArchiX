using ArchiX.Library.Abstractions.Connections;

namespace ArchiX.Library.Runtime.Connections;

internal sealed class EnvSecretResolver : ISecretResolver
{
    public ValueTask<string?> TryResolveAsync(string secretRef, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(secretRef)) return ValueTask.FromResult<string?>(null);

        var trimmed = secretRef.Trim();
        if (!trimmed.StartsWith("ENV:", StringComparison.OrdinalIgnoreCase))
            return ValueTask.FromResult<string?>(null);

        var name = trimmed["ENV:".Length..].Trim();
        if (string.IsNullOrWhiteSpace(name)) return ValueTask.FromResult<string?>(null);

        return ValueTask.FromResult(Environment.GetEnvironmentVariable(name));
    }
}
