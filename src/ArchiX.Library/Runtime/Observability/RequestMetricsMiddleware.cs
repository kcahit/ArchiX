// File: src/ArchiX.Library/Runtime/Observability/RequestMetricsMiddleware.cs
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ArchiX.Library.Runtime.Observability;

/// <summary>
/// HTTP isteklerinin süresini ve sonuçlarını <see cref="RequestMetric"/> ile kaydeder.
/// Route bilgisini mümkünse <see cref="RouteEndpoint.RoutePattern"/> üzerinden alır.
/// </summary>
public sealed class RequestMetricsMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Middleware örneği oluşturur.
    /// </summary>
    /// <param name="next">Zincirdeki bir sonraki middleware.</param>
    /// <exception cref="ArgumentNullException">next null ise fırlatılır.</exception>
    public RequestMetricsMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// HTTP isteğini işler ve metrikleri kaydeder.
    /// </summary>
    /// <param name="context">Geçerli HTTP bağlamı.</param>
    public async Task Invoke(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
        }
        finally
        {
            var elapsedMs = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
            var method = context.Request.Method;
            var status = context.Response.StatusCode;
            var route = ResolveRoute(context);
            RequestMetric.Record(method, route, status, elapsedMs);
        }
    }

    /// <summary>
    /// Route bilgisini belirler. Öncelik: Raw route pattern → adlandırılmış rota → endpoint adı → path.
    /// Hiçbir zaman null dönmez.
    /// </summary>
    private static string ResolveRoute(HttpContext context)
    {
        var ep = context.GetEndpoint();

        if (ep is RouteEndpoint rep)
        {
            var raw = rep.RoutePattern?.RawText;
            if (!string.IsNullOrWhiteSpace(raw))
                return raw!;
        }

        var named = ep?.Metadata.GetMetadata<RouteNameMetadata>()?.RouteName;
        if (!string.IsNullOrWhiteSpace(named))
            return named!;

        var display = ep?.DisplayName;
        if (!string.IsNullOrWhiteSpace(display))
            return display!;

        var path = context.Request.Path.Value;
        return string.IsNullOrWhiteSpace(path) ? "/" : path!;
    }
}
