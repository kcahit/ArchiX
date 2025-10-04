// File: tests/ArchiXTest.ApiWeb/Test/RunTimeTests/ObservabilityTests/LoggingScopeMiddlewareTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RunTimeTests.ObservabilityTests;

/// <summary>LoggingScopeMiddleware’in log scope’a trace_id ve span_id eklediğini doğrular.</summary>
public sealed class LoggingScopeMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly InMemoryLoggerProvider _provider = new();

    public LoggingScopeMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSolutionRelativeContentRoot("tests/ArchiXTest.ApiWeb");
            builder.UseSetting("DOTNET_ENVIRONMENT", "Testing");

            // Tracing ve Metrics açık
            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                var inmem = new Dictionary<string, string?>
                {
                    ["Observability:Enabled"] = "true",
                    ["Observability:Tracing:Enabled"] = "true",
                    ["Observability:Tracing:Exporter"] = "none",
                    ["Observability:Metrics:Enabled"] = "true",
                    ["Observability:Metrics:Exporter"] = "prometheus",
                    ["Observability:Metrics:ScrapeEndpoint"] = "/metrics"
                };
                cfg.AddInMemoryCollection(inmem!);
            });

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(_provider);
                logging.SetMinimumLevel(LogLevel.Information);
            });

            // İstek içinde mutlaka bir log üret (scope’u yakalamak için)
            builder.Configure(app =>
            {
                app.Use(async (ctx, next) =>
                {
                    var logger = ctx.RequestServices.GetRequiredService<ILogger<LoggingScopeMiddlewareTests>>();
                    logger.LogInformation("probe-log");
                    await next();
                });
            });
        });
    }

    [Fact]
    public async Task Scope_Should_Contain_Trace_And_Span_Ids()
    {
        var client = _factory.CreateClient();

        // HealthCheck yerine gerçek action
        _ = await client.GetAsync("/ping/status");

        var scope = _provider.LastScope;
        Assert.NotNull(scope);

        var hasTrace = scope!.ContainsKey("trace_id") || scope.ContainsKey("TraceId");
        var hasSpan = scope.ContainsKey("span_id") || scope.ContainsKey("SpanId");

        Assert.True(hasTrace);
        Assert.True(hasSpan);

        var traceVal = scope.TryGetValue("trace_id", out var t1) ? t1?.ToString()
                     : scope.TryGetValue("TraceId", out var t2) ? t2?.ToString()
                     : null;

        var spanVal = scope.TryGetValue("span_id", out var s1) ? s1?.ToString()
                    : scope.TryGetValue("SpanId", out var s2) ? s2?.ToString()
                    : null;

        Assert.False(string.IsNullOrWhiteSpace(traceVal));
        Assert.False(string.IsNullOrWhiteSpace(spanVal));
    }

    /// <summary>Log çağrılarında external scope’tan değerleri toplayan provider.</summary>
    private sealed class InMemoryLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider? _scopeProvider;
        public Dictionary<string, object>? LastScope { get; private set; }

        public ILogger CreateLogger(string categoryName) => new InMemoryLogger(this);
        public void Dispose() { }
        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

        private sealed class InMemoryLogger : ILogger
        {
            private readonly InMemoryLoggerProvider _owner;
            public InMemoryLogger(InMemoryLoggerProvider owner) => _owner = owner;

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (_owner._scopeProvider is null) return;

                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _owner._scopeProvider.ForEachScope<object?>((scopeObj, _) =>
                {
                    if (scopeObj is IEnumerable<KeyValuePair<string, object>> kvs)
                        foreach (var kv in kvs) dict[kv.Key] = kv.Value!;
                }, null);

                if (dict.Count > 0) _owner.LastScope = dict;
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }
    }
}
