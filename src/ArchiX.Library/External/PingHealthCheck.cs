// File: src/ArchiX.Library/External/PingHealthCheck.cs
#nullable enable
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ArchiX.Library.External
{
    /// <summary><see cref="IPingAdapter"/> üzerinden dış servisin sağlık kontrolü.</summary>
    /// <param name="adapter">Ping işlemlerini sağlayan bağımlılık.</param>
    public sealed class PingHealthCheck(IPingAdapter adapter) : IHealthCheck
    {
        private readonly IPingAdapter _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

        /// <summary>Ping çağrısı yapar ve sonucu <see cref="HealthCheckResult"/> olarak döner.</summary>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                var text = await _adapter.GetStatusTextAsync(cts.Token).ConfigureAwait(false);
                var len = text?.Length ?? 0;
                return HealthCheckResult.Healthy($"Ping OK (len={len})");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Ping FAILED", ex);
            }
        }
    }
}
