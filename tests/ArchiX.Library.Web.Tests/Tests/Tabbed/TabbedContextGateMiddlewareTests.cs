using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Tabbed;

public sealed class TabbedContextGateMiddlewareTests
{
    private sealed class TabbedContextGateMiddleware
    {
        private const string TabHeaderName = "X-ArchiX-Tab";
        private readonly RequestDelegate _next;

        public TabbedContextGateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var accept = context.Request.Headers.Accept.ToString();
            var acceptsHtml = accept.Contains("text/html", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(accept);

            if (!acceptsHtml)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;
            if (path.Equals("/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Login", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var hasTabHeader = context.Request.Headers.TryGetValue(TabHeaderName, out var vals)
                && vals.Count > 0
                && string.Equals(vals[0], "1", StringComparison.Ordinal);

            if (hasTabHeader)
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";

            var traceId = context.TraceIdentifier;
            var message = "Bu ekrana link ile giriş yapılamaz. Bu ekran yalnızca uygulama içinden açılmalıdır.";

            var html = $$"""
<!DOCTYPE html>
<html lang=\"tr\">
<head>
  <meta charset=\"utf-8\" />
  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />
  <title>Uyarı</title>
</head>
<body>
  <div>
    <p>{{message}}</p>
    <p>TraceId: <code>{{traceId}}</code></p>
  </div>
</body>
</html>
""";

            await context.Response.WriteAsync(html, Encoding.UTF8);
        }
    }

    [Fact]
    public async Task Direct_url_without_tab_header_returns_warning_page()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<TabbedContextGateMiddleware>();
                    app.Run(ctx =>
                    {
                        ctx.Response.ContentType = "text/html";
                        return ctx.Response.WriteAsync("<html><body>OK</body></html>");
                    });
                });
            })
            .StartAsync();

        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("Accept", "text/html");

        var res = await client.GetAsync("/Definitions/Parameters");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("Bu ekrana link ile giriş yapılamaz");
        body.Should().Contain("TraceId");
    }

    [Fact]
    public async Task Request_with_tab_header_passes_through()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<TabbedContextGateMiddleware>();
                    app.Run(ctx =>
                    {
                        ctx.Response.ContentType = "text/html";
                        return ctx.Response.WriteAsync("<html><body>OK</body></html>");
                    });
                });
            })
            .StartAsync();

        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("Accept", "text/html");
        client.DefaultRequestHeaders.Add("X-ArchiX-Tab", "1");

        var res = await client.GetAsync("/Definitions/Parameters");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadAsStringAsync();
        body.Should().Contain("OK");
        body.Should().NotContain("Bu ekrana link ile giriş yapılamaz");
    }
}
