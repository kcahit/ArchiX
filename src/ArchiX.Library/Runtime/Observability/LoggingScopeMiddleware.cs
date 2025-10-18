using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// Geçerli <see cref="Activity"/>’den trace_id ve span_id’yi log scope’a ekler.
/// Activity yoksa geçici bir Activity başlatır.
/// </summary>
public sealed class LoggingScopeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingScopeMiddleware> _logger;

    /// <summary>
    /// Yeni <see cref="LoggingScopeMiddleware"/> oluşturur.
    /// </summary>
    /// <param name="next">Pipeline’daki bir sonraki bileşen.</param>
    /// <param name="logger">Günlükleyici.</param>
    public LoggingScopeMiddleware(RequestDelegate next, ILogger<LoggingScopeMiddleware> logger)
    {
        _next = next ?? throw new System.ArgumentNullException(nameof(next));
        _logger = logger;
    }

    /// <summary>
    /// HTTP isteğini işler ve log scope zenginleştirmesini uygular.
    /// </summary>
    /// <param name="context">Geçerli HTTP bağlamı.</param>
    public async Task Invoke(HttpContext context)
    {
        var act = Activity.Current;
        var started = false;

        if (act is null)
        {
            act = ArchiXTelemetry.Activity.StartActivity("Request");
            started = act is not null;
        }

        var scopeState = new Dictionary<string, object>
        {
            ["trace_id"] = act?.TraceId.ToString() ?? context.TraceIdentifier,
            ["span_id"] = act?.SpanId.ToString() ?? "0000000000000000"
        };

        using (_logger.BeginScope(scopeState))
        {
            try
            {
                // Testler scope’u bir log çağrısı sırasında okur.
                _logger.LogInformation("obs.scope.active");
                await _next(context);
            }
            finally
            {
                if (started) act?.Dispose();
            }
        }
    }
}
