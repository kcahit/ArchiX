namespace ArchiX.Library.Abstractions.ConnectionPolicy
{
    /// <summary>Cached access to ConnectionPolicyOptions.</summary>
    public interface IConnectionPolicyProvider
    {
        ConnectionPolicyOptions Current { get; }
        void ForceRefresh();
    }
}
