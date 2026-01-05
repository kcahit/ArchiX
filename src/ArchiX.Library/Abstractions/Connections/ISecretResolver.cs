namespace ArchiX.Library.Abstractions.Connections;

public interface ISecretResolver
{
    ValueTask<string?> TryResolveAsync(string secretRef, CancellationToken ct = default);
}
