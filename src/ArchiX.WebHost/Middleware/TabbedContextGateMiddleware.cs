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
        // Only guard GET navigations.
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Only guard Razor Pages navigations (HTML). API/JSON requests are not in scope here.
        var accept = context.Request.Headers.Accept.ToString();
        var acceptsHtml = accept.Contains("text/html", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(accept);

        if (!acceptsHtml)
        {
            await _next(context);
            return;
        }

        // Allow auth/landing pages and error page.
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.Equals("/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Dashboard", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // After a successful login, a redirect to a local ReturnUrl may not carry the tab header.
        // Treat same-origin Referer or explicit ReturnUrl-based navigations as "in-app" to avoid false positives.
        var hasReturnUrl = context.Request.Query.ContainsKey("returnUrl")
            || context.Request.Query.ContainsKey("ReturnUrl");

        // If it's a direct browser navigation, we only block when there is a Referer from our own site absent.
        // This prevents false positives after login redirects.
        var hasSameOriginReferer = false;
        if (context.Request.Headers.TryGetValue("Referer", out var refererVals))
        {
            var referer = refererVals.ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var r)
                && string.Equals(r.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
            {
                hasSameOriginReferer = true;
            }
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

        // If user is navigating inside the app (same-origin referer), do not hard-block.
        if (hasSameOriginReferer || hasReturnUrl)
        {
            await _next(context);
            return;
        }

        // Direct URL navigation: show a minimal response card (Close only).
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
        <h5 class=\"card-title\">Ýþlem tamamlanamadý (200)</h5>
        <p class=\"card-text\">{{message}}</p>
        <hr />
        <div class=\"d-flex gap-2 flex-wrap\">
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
