namespace ArchiX.Library.Abstractions.ConnectionPolicy
{
    public interface IConnectionPolicyAuditor
    {
        // Non-blocking audit write (fire-and-forget)
        void TryWrite(string rawConnectionString, ConnectionPolicyResult result);
    }
}