namespace ArchiX.Library.Abstractions.ConnectionPolicy
{
    public interface IConnectionPolicyEvaluator
    {
        /// <summary>
        /// C-1: Sadece Encrypt/TrustServerCertificate/Integrated Security kurallarýna göre deðerlendirir.
        /// Whitelist ve audit C-2/C-3’te gelecektir.
        /// </summary>
        ConnectionPolicyResult Evaluate(string connectionString);
    }
}