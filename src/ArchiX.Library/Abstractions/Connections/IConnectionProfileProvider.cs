namespace ArchiX.Library.Abstractions.Connections;

public interface IConnectionProfileProvider
{
    ValueTask<ConnectionProfile> GetProfileAsync(string connectionName, CancellationToken ct = default);
}
