using System.Text;

using Microsoft.AspNetCore.Http;

namespace ArchiX.WebHost.Middleware;

public sealed class TabbedContextGateMiddleware
{
    private const string TabHeaderName = "X-ArchiX-Tab";
    private readonly RequestDelegate _next;

    public TabbedContextGateMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task Invoke(HttpContext context)
    {
        // Only guard Razor Pages navigations (HTML). API/JSON requests are not in scope here.
        var acceptsHtml = context.Request.Headers.Accept.Any(v => v.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            || string.IsNullOrWhiteSpace(context.Request.Headers.Accept);

        if (!acceptsHtml)
        {
            await _next(context);
            return;
        }

        // Allow auth/landing pages and error page.
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.Equals("/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // If request comes from TabHost, header must exist.
        // This is a UX gate (not a security boundary).
        var hasTabHeader = context.Request.Headers.TryGetValue(TabHeaderName, out var vals)
            && vals.Count > 0
            && string.Equals(vals[0], "1", StringComparison.Ordinal);

        if (hasTabHeader)
        {
            await _next(context);
            return;
        }

        // Direct URL navigation: return a minimal HTML response with only Close button.
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/html; charset=utf-8";

        var traceId = context.TraceIdentifier;
        var message = "Bu ekrana link ile giriþ yapýlamaz. Bu ekran yalnýzca uygulama içinden açýlmalýdýr.";

        var html = $$"""
<!DOCTYPE html>
<html lang=\"tr\">
<head>
  <meta charset=\"utf-8\" />
  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />
  <title>Uyarý</title>
  <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css\" rel=\"stylesheet\" />
</head>
<body>
  <div class=\"container mt-4\">
    <div class=\"card shadow-sm\">
      <div class=\"card-body\">
        <h5 class=\"card-title\">Uyarý</h5>
        <p class=\"card-text\">{{message}}</p>
        <div class=\"mt-3\">
          <button type=\"button\" class=\"btn btn-secondary\" onclick=\"history.back()\">Kapat</button>
        </div>
        <div class=\"mt-2 small text-muted\">TraceId: <code>{{traceId}}</code></div>
      </div>
    </div>
  </div>
</body>
</html>
""";

        await context.Response.WriteAsync(html, Encoding.UTF8);
     }
 }
