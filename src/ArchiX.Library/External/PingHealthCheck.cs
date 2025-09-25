// File: src/ArchiX.Library/External/PingHealthCheck.cs
#nullable enable
namespace ArchiX.Library.External
{
    /// <summary>IPingAdapter üzerinden dış servisin sağlık kontrolü.</summary>
    /// <param name="adapter">Ping işlemlerini sağlayan bağımlılık.</param>
    public sealed class PingHealthCheck(IPingAdapter adapter) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck
    {
        private readonly IPingAdapter _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

        /// <summary>Ping çağrısı yapar ve sonucu HealthCheckResult olarak döner.</summary>
        public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(
            Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                var text = await _adapter.GetStatusTextAsync(cts.Token).ConfigureAwait(false);
                var len = text?.Length ?? 0;
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Ping OK (len={len})");
            }
            catch (Exception ex)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Ping FAILED", ex);
            }
        }
    }
}
